using ErgoShop.POCO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crosstales.FB;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Converters;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using ErgoShop.Operations;
using ErgoShop.Utils;
using odf = Independentsoft.Office.Odf;
using Independentsoft.Office.Odf.Drawing;
using Independentsoft.Office.Odf.Styles;
using Independentsoft.Office.Odf;
using ErgoShop.UI;
using ErgoShop.Cameras;

namespace ErgoShop.Managers
{
    /// <summary>
    /// Manager to handle file load and save. Uses Newtonsoft.Json
    /// </summary>
    public class ProjectManager : MonoBehaviour
    {
        /// <summary>
        /// Current Project
        /// </summary>
        public Project Project { get; set; }

        private string filePath;
        private string m_currentFilePath;

        private string m_folderAutoSave;

        private bool m_isExporting = false;

        public ConfirmationPopinScript popin;
        public NewProjectConfirmationPopin newpopin;

        public static ProjectManager Instance;

        /// <summary>
        /// Catch any user input to quit the software (with ALT+F4 or the close button)
        /// And checks if diplay save warning or not
        /// </summary>
        /// <returns></returns>
        static bool WantsToQuit()
        {
            ProjectManager.Instance.popin.gameObject.SetActive(true);
            if (ProjectManager.Instance.popin.goQuit)
            {
                SettingsManager.Instance.SaveParameters();
                return true;
            }
            return false;
        }

        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            Application.wantsToQuit += WantsToQuit;
        }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            Project = new Project();
            Project.Person = new Person();
            Project.Floors = new List<ErgoShop.POCO.Floor>();
            Project.Floors.Add(new ErgoShop.POCO.Floor
            {
                FloorName = "RDC",
                Furnitures = new List<Furniture>(),
                Walls = new List<Wall>(),
                Rooms = new List<Room>(),
            });
            Project.WallHeight = 3;
            Project.WallThickness = 0.1f;

            // APPDATA
            m_folderAutoSave =
                System.IO.Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData), "ErgoShop", "AutoSave");

            if (Directory.Exists(m_folderAutoSave))
            {
                DirectoryInfo di = new DirectoryInfo(m_folderAutoSave);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                Directory.Delete(m_folderAutoSave);
            }
        }


        // Update is called once per frame
        void Update()
        {
            // CTRL + S = save
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    SaveCurrentFile();
                }
            }
        }

        /// <summary>
        /// UI popin to confirm the project change
        /// </summary>
        /// <param name="gonew"></param>
        private void ShowConfirmationPopin(bool gonew)
        {
            newpopin.gameObject.SetActive(true);
            newpopin.goNew = gonew;
        }


        public void SetNewProject()
        {
            filePath = string.Empty;
            m_currentFilePath = string.Empty;
        }

        /// <summary>
        /// Restart software after confirmation
        /// </summary>
        public void NewFile()
        {
            if (IsLastSaveChanged())
            {
                ShowConfirmationPopin(true);
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }

        /// <summary>
        /// Check if auto save if different from last save
        /// </summary>
        /// <returns></returns>
        private bool IsLastSaveChanged()
        {
            if (m_currentFilePath == null) return AllElementsManager.Instance.AllElements.Count > 0;
            string currentProjectContent = File.ReadAllText(m_currentFilePath);
            string lastAutoSaveContent = OperationsBufferScript.Instance.GetLastAutoSaveContent();
            return !String.Equals(currentProjectContent, lastAutoSaveContent);
        }

        /// <summary>
        /// Show confirmation then load a file into project
        /// </summary>
        public void LoadProject()
        {
            if (IsLastSaveChanged())
            {
                ShowConfirmationPopin(false);
            }
            else
            {
                LoadProjectForReal();
            }
        }

        /// <summary>
        /// Export project into odt file
        /// </summary>
        public void ExportProjectOnODTFile()
        {
            string path = FileBrowser.SaveFile("export", "odt");
            StartCoroutine(ExportToOdt(path));
            UIManager.Instance.ShowCustomMessage("Export en cours... Veuillez patienter", 2f+0.3f*Project.Floors.Sum(f=>f.Rooms.Count)*5f);
        }

        public bool IsOccupied()
        {
            return m_isExporting;
        }

        /// <summary>
        /// Build the odt file
        /// </summary>
        /// <param name="filePath"></param>
        public IEnumerator ExportToOdt(string filePath)
        {
            m_isExporting = true;
            // FIRST, GENERATE SCREENSHOTS
            string folder = System.IO.Path.GetDirectoryName(filePath);
            string name = System.IO.Path.GetFileNameWithoutExtension(filePath);
            string imagesFolder = System.IO.Path.Combine(folder, name + "_images");
            Directory.CreateDirectory(imagesFolder);
            System.IO.DirectoryInfo di = new DirectoryInfo(imagesFolder);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            float x = Project.GetCurrentFloor().Rooms.Average(r => r.GetCenter().x);
            float y = Project.GetCurrentFloor().Rooms.Average(r => r.GetCenter().y);
            float z = Project.GetCurrentFloor().Rooms.Average(r => r.GetCenter().z);

            yield return new WaitForSeconds(0.3f);
            string whole2D = System.IO.Path.Combine(imagesFolder, "whole2D.png");
            yield return Screenshot.Instance.TakeBig2DScreenshot(imagesFolder, new Vector3(x, y+50, z), "whole2D");
            yield return new WaitForSeconds(0.3f);
            string whole3D = Screenshot.Instance.TakeBig3DScreenshot(imagesFolder, new Vector3(x, y+50, z), "whole3D");
            yield return new WaitForSeconds(0.3f);

            Image whole2DImage = new Image(whole2D);

            Frame whole2DFrame = new Frame();
            whole2DFrame.Width = new Size(1920 / 4f, Unit.Pixel);
            whole2DFrame.Height = new Size(1080 / 4f, Unit.Pixel);
            whole2DFrame.Add(whole2DImage);

            Image whole3DImage = new Image(whole3D);

            Frame whole3DFrame = new Frame();
            whole3DFrame.Width = new Size(1920 / 4f, Unit.Pixel);
            whole3DFrame.Height = new Size(1080 / 4f, Unit.Pixel);
            whole3DFrame.Add(whole3DImage);

            // ROOM SCREENSHOTS
            List<Image> roomImages = new List<Image>();
            List<Frame> room2DFrames = new List<Frame>();
            List<Frame> room3DFrames = new List<Frame>();

            int cpt = 0;
            foreach(var f in Project.Floors)
            {
                yield return new WaitForSeconds(0.3f);
                SetCurrentFloor(Project.Floors.IndexOf(f));
                yield return new WaitForSeconds(0.3f);
                int cpt2 = 0;
                foreach(Room ro in f.Rooms)
                {
                    string room2D = System.IO.Path.Combine(imagesFolder, ro.Name + "_" + cpt + "_" + cpt2 + "2D.png");
                    Vector3 roomCenter = VectorFunctions.Switch2D3D(ro.GetCenter());
                    yield return Screenshot.Instance.TakeBig2DScreenshot(imagesFolder, roomCenter, ro.Name + "_" + cpt + "_" + cpt2 + "2D");
                    yield return new WaitForSeconds(0.3f);
                    string room3D = Screenshot.Instance.TakeBig3DScreenshot(imagesFolder, roomCenter, ro.Name + "_" + cpt + "_" + cpt2 + "3D");
                    yield return new WaitForSeconds(0.3f);
                    
                    Image room2DImage = new Image(room2D);
                    Frame room2DFrame = new Frame();
                    room2DFrame.Width = new Size(1920 / 4f, Unit.Pixel);
                    room2DFrame.Height = new Size(1080 / 4f, Unit.Pixel);
                    room2DFrame.Add(room2DImage);
                    room2DFrames.Add(room2DFrame);

                    Image room3DImage = new Image(room3D);
                    Frame room3DFrame = new Frame();
                    room3DFrame.Width = new Size(1920 / 4f, Unit.Pixel);
                    room3DFrame.Height = new Size(1080 / 4f, Unit.Pixel);
                    room3DFrame.Add(room3DImage);
                    room3DFrames.Add(room3DFrame);

                    roomImages.Add(room2DImage);
                    roomImages.Add(room3DImage);

                    cpt2++;

                    yield return new WaitForSeconds(0.5f);
                }
                cpt++;
            }

            // THEN, GENERATE DOC
            TextDocument doc = new TextDocument();

            // FONTS
            odf.Styles.Font arial = new odf.Styles.Font();
            arial.Name = "Arial";
            arial.Family = "Arial";
            arial.GenericFontFamily = GenericFontFamily.Swiss;
            arial.Pitch = FontPitch.Variable;

            doc.Fonts.Add(arial);
            // STYLES
            ParagraphStyle arial12Style = new ParagraphStyle("Arial12Style");
            arial12Style.TextProperties.Font = "Arial";
            arial12Style.TextProperties.FontSize = new Size(12, Unit.Point);

            ParagraphStyle titleStyle = new ParagraphStyle("TitleStyle");
            titleStyle.ParagraphProperties.TextAlignment = odf.Styles.TextAlignment.Center;            
            titleStyle.TextProperties.Font = "Arial";
            titleStyle.TextProperties.FontSize = new Size(16, Unit.Point);
            titleStyle.TextProperties.FontWeight = FontWeight.Bold;
            titleStyle.ParagraphProperties.BreakBefore = Break.Page;


            ParagraphStyle subtitleStyle = new ParagraphStyle("SubTitleStyle");
            subtitleStyle.ParagraphProperties.TextAlignment = odf.Styles.TextAlignment.Left;            
            subtitleStyle.TextProperties.Font = "Arial";
            subtitleStyle.TextProperties.FontSize = new Size(12, Unit.Point);
            subtitleStyle.TextProperties.FontWeight = FontWeight.Bold;
            subtitleStyle.TextProperties.UnderlineType = UnderlineType.Single;

            ParagraphStyle breakPage = new ParagraphStyle("BreakPage");
            breakPage.ParagraphProperties.BreakBefore = Break.Page;

            CellStyle cs1 = new CellStyle("CS1");
            cs1.TextProperties.Color = "#555555";

            doc.AutomaticStyles.Styles.Add(titleStyle);
            doc.AutomaticStyles.Styles.Add(arial12Style);
            doc.AutomaticStyles.Styles.Add(subtitleStyle);
            doc.AutomaticStyles.Styles.Add(breakPage);
            doc.AutomaticStyles.Styles.Add(cs1);


            // CONTENT
            // FIRST PAGE                
            Paragraph p1 = new Paragraph
            {
                Style = "Arial12Style"
            };
            p1.Add(Project.ProjectName);
            p1.AddLineBreak();
            p1.Add(Project.Person.FirstName + " " + Project.Person.LastName);

            // SECOND PAGE
            Paragraph p2 = new Paragraph
            {
                Style = "TitleStyle"
            };            
            p2.Add("PRECONISATIONS D'AMENAGEMENT DU LOGEMENT");
            p2.AddLineBreak();
            p2.AddLineBreak();
            Paragraph p3 = new Paragraph
            {
                Style = "SubTitleStyle"
            };
            p3.Add("Le bénéficiaire");

            Paragraph p4 = new Paragraph
            {
                Style = "Arial12Style"
            };
            p4.Add("Projet : " + Project.ProjectName);
            p4.AddLineBreak();
            p4.Add("Date : " + Project.Date.ToShortDateString());
            p4.AddLineBreak();
            p4.Add("Bénéficiaire : " + Project.Person.FirstName + " " + Project.Person.LastName);
            p4.AddLineBreak();
            p4.Add("Version : v." + Project.Version);
            p4.AddLineBreak();
            p4.Add("Commentaire : " + Project.Comment);
            p4.AddLineBreak();
            p4.AddLineBreak();
            p4.AddLineBreak();

            Paragraph p5 = new Paragraph
            {
                Style = "SubTitleStyle"
            };
            p5.Add("La modélisation");

            Paragraph p6 = new Paragraph
            {
                Style = "Arial12Style"
            };
            p6.Add("Type de logement : " + Project.HomeType);
            p6.AddLineBreak();
            p6.Add("Nombre d'étages : " + Project.Floors.Count);
            p6.AddLineBreak();
            p6.Add("Nombre de pièces total : " + Project.Floors.Sum(f => f.Rooms.Count()));
            p6.AddLineBreak();
            p6.Add("Superficie totale : ");
            p6.AddLineBreak();

            // ss
            p6.Add(whole2DFrame);
            p6.AddLineBreak();
            p6.AddLineBreak();
            p6.AddLineBreak();

            // ss
            p6.Add(whole3DFrame);
            p6.AddLineBreak();

            // ROOMS
            Paragraph pBreakPage = new Paragraph();
            pBreakPage.Style = "BreakPage";
            pBreakPage.Add("");

            Paragraph p7 = new Paragraph();
            p7.Style = "SubTitleStyle";
            p7.Add("Les pièces");

            List<Paragraph> roomsParagraphs = new List<Paragraph>();
            cpt = 1;
            foreach (var f in Project.Floors)
            {
                foreach (var r in f.Rooms) {
                    Paragraph curParaTitle = new Paragraph();
                    curParaTitle.Style = "SubTitleStyle";
                    curParaTitle.AddTab();
                    curParaTitle.Add("Pièce n°" + cpt + " : " + r.Name);
                    curParaTitle.AddLineBreak();
                    curParaTitle.AddTab();
                    curParaTitle.AddTab();
                    curParaTitle.Add("Présentation");

                    Paragraph curPara = new Paragraph();

                    curPara.Add("Etage : " + f.FloorName);
                    curPara.AddLineBreak();
                    curPara.Add("Nombre de murs : " + r.Walls.Count);
                    curPara.AddLineBreak();
                    curPara.Add("Epaisseur des murs : ");
                    string ths = "";
                    foreach(var w in r.Walls)
                    {
                        ths += w.Thickness + "m, ";
                    }
                    curPara.Add(ths);
                    curPara.AddLineBreak();

                    curPara.Add(room2DFrames[cpt - 1]);
                    curPara.Add(room3DFrames[cpt - 1]);
                    curPara.AddLineBreak();

                    roomsParagraphs.Add(curParaTitle);
                    roomsParagraphs.Add(curPara);
                    
                    cpt++;
                }



                // MEUBLES
                Paragraph furniTitle = new Paragraph();
                furniTitle.Style = "SubTitleStyle";
                furniTitle.AddTab();
                furniTitle.AddTab();
                furniTitle.Add("Meubles");

                roomsParagraphs.Add(furniTitle);
                foreach (var fu in f.Furnitures)
                {
                    if(fu.Type != "AideTechnique")
                    {
                        Paragraph p = new Paragraph();
                        p.Style = "Arial12Style";
                        p.Add(fu.GetDescription());
                        roomsParagraphs.Add(p);
                    }
                }

                // AIDES TECHNIQUES
                Paragraph furni2Title = new Paragraph();
                furni2Title.Style = "SubTitleStyle";
                furni2Title.AddTab();
                furni2Title.AddTab();
                furni2Title.Add("Aides techniques");

                roomsParagraphs.Add(furni2Title);
                foreach (var fu in f.Furnitures)
                {
                    if (fu.Type == "AideTechnique")
                    {
                        Paragraph p = new Paragraph();
                        p.Style = "Arial12Style";
                        p.Add(fu.GetDescription());
                        roomsParagraphs.Add(p);
                    }
                }

                // COMMENTS
                Paragraph comTitle = new Paragraph();
                comTitle.Style = "SubTitleStyle";
                comTitle.AddTab();
                comTitle.AddTab();
                comTitle.Add("Commentaires");

                roomsParagraphs.Add(comTitle);
                foreach (var tz in f.TextZoneElements)
                {
                        Paragraph p = new Paragraph();
                        p.Style = "Arial12Style";
                        p.Add(tz.GetDescription());
                        roomsParagraphs.Add(p);
                }

                // MOBILITE
                Paragraph chTitle = new Paragraph();
                chTitle.Style = "SubTitleStyle";
                chTitle.AddTab();
                chTitle.AddTab();
                chTitle.Add("Mobilité");

                roomsParagraphs.Add(chTitle);
                foreach (var ch in f.Characters)
                {
                    Paragraph p = new Paragraph();
                    p.Style = "Arial12Style";
                    p.Add(ch.GetDescription());
                    roomsParagraphs.Add(p);
                }
            }

            //====================================
            // POUR RESUMER

            Paragraph ps1 = new Paragraph();
            ps1.Style = "SubTitleStyle";
            ps1.Add("Pour résumer");
            Paragraph ps2 = new Paragraph();
            ps2.Style = "Arial12Style";
            ps2.Add("Nombre de pièces : " + Project.Floors.Sum(f => f.Rooms.Count()));
            ps2.AddLineBreak();
            string pieces = "";
            foreach (var f in Project.Floors)
            {
                foreach (var r in f.Rooms)
                {
                    pieces += r.Name + ", ";
                }
            }
            ps2.Add("Pièces : " + pieces);
            ps2.AddLineBreak();

            Cell cell1 = new Cell("Aide technique");
            Cell cell2 = new Cell("Pièce associée");
            Cell cell3 = new Cell("Commentaire");
            cell1.Style = "CS1";
            cell2.Style = "CS1";
            cell3.Style = "CS1";

            Row row1 = new Row();
            row1.Cells.Add(cell1);
            row1.Cells.Add(cell2);
            row1.Cells.Add(cell3);

            Table table1 = new Table();
            table1.Rows.Add(row1);

            foreach(var f in Project.Floors)
            {
                foreach(var fu in f.Furnitures)
                {
                    if(fu.Type == "AideTechnique")
                    {
                        Row ro = new Row();
                        ro.Cells.Add(new Cell(fu.Name));
                        ro.Cells.Add(new Cell(""));
                        ro.Cells.Add(new Cell(""));
                        table1.Rows.Add(ro);
                    }
                }
            }

            // If table is empty, add an empty line
            if(table1.Rows.Count == 1)
            {
                Row ro = new Row();
                ro.Cells.Add(new Cell(""));
                ro.Cells.Add(new Cell(""));
                ro.Cells.Add(new Cell(""));
                table1.Rows.Add(ro);
            }

            Paragraph ps3 = new Paragraph();
            ps3.Style = "Arial12Style";
            ps3.Add("[nom_preconisateur] ");
            ps3.AddTab();
            ps3.Add(DateTime.Now.ToShortDateString());
            ps3.AddTab();
            ps3.Add("Version ErgoShop " + GlobalManager.VERSION);
            ps3.AddLineBreak();


            // DOC CONSTRUCTION
            doc.Body.Add(p1);            
            doc.Body.Add(p2);
            doc.Body.Add(p3);
            doc.Body.Add(p4);
            doc.Body.Add(p5);
            doc.Body.Add(p6);
            doc.Body.Add(pBreakPage);            
            doc.Body.Add(p7);

            foreach (var p in roomsParagraphs)
            {
                doc.Body.Add(p);
            }

            doc.Body.Add(pBreakPage);
            doc.Body.Add(ps1);
            doc.Body.Add(ps2);
            doc.Body.Add(table1);
            doc.Body.Add(ps3);

            doc.Save(filePath, true);

            UIManager.Instance.ShowCustomMessage("FICHIER ODT EXPORTE");
            yield return new WaitForSeconds(0.2f);
            GlobalManager.Instance.SwitchViewMode();
            yield return new WaitForSeconds(0.2f);
            GlobalManager.Instance.Set2DTopMode();
            m_isExporting = false;
        }

        /// <summary>
        /// Export project into a txt file
        /// </summary>
        public void ExportProjectOnTxtFile()
        {
            string txt = GetExportTxt();
            string path = FileBrowser.SaveFile("export", "txt");
            File.WriteAllText(path, txt);
            UIManager.Instance.ShowCustomMessage("Fichier texte exporté.");
        }

        /// <summary>
        /// Build the txt file
        /// </summary>
        /// <returns>string containing all descriptions and data</returns>
        public string GetExportTxt()
        {
            string res = "";
            res += "===================================================\n";
            res += "==== PROJET " + Project.ProjectName + " ====\n\n";
            res += "===================================================\n";
            res += "[nom_preconisateur] " + DateTime.Now.ToShortDateString() + " version ergoshop : " + GlobalManager.VERSION + "\n";
            res += "Date : " + Project.Date.ToShortDateString() + "\n";
            res += "Bénéficiaire " + Project.Person.FirstName + " " + Project.Person.LastName + "\n";
            res += "Version : v." + Project.Version + "\n";
            res += "Commentaire : " + Project.Comment + "\n\n";
            res += "==== MODELISATION ==== \n\n";
            res += "Type de logement : " + Project.HomeType + "\n";
            res += "Nombre d'étages : " + Project.Floors.Count + " ===\n";
            res += "Nombre de pièce total : " + Project.Floors.Sum(f => f.Rooms.Count());
            res += "Superficie totale : " + "[superficie_totale]\n";
            res += "Mobilité : Détaillée dans les pièces :\n";
            res += "===================================================\n";
            res += "================== LES PIECES =====================\n\n";
            res += "===================================================\n";
            foreach (var f in Project.Floors)
            {
                //res += "== " + f.FloorName + "==\n";
                //res += "Pieces : " + f.Rooms.Count + "\n";
                int cptpiece = 0;
                foreach (var r in f.Rooms)
                {
                    cptpiece++;
                    res += "**************** PIECE " + cptpiece + " ***************\n";
                    res += "Etage : " + f.FloorName + "\n";
                    res += r.GetDescription();
                }

                res += "========= MOBILITE =========\n";
                foreach (var ch in f.Characters)
                {
                    res += ch.GetDescription();
                }
                res += "========= MEUBLES =========\n";
                foreach (var fu in f.Furnitures)
                {
                    if(fu.Type != "AideTechnique") {
                        res += fu.GetDescription();
                    }
                }
                foreach (var sta in f.Stairs)
                {
                    res += sta.GetDescription();
                }
                res += "========= AIDES TECHNIQUES =========\n";
                foreach (var fu in f.Furnitures)
                {
                    if (fu.Type == "AideTechnique")
                    {
                        res += fu.GetDescription();
                    }
                }
                res += "========= COMMENTAIRES ========= \n";
                foreach (var text in f.TextZoneElements)
                {
                    res += text.GetDescription();
                }

            }
            res += "===================================================\n";
            res += "================== POUR RESUMER =====================\n\n";
            res += "===================================================\n\n\n\n";
            res += "Nombre de pièces : " + Project.Floors.Sum(f => f.Rooms.Count()) + "\n";
            res += "Pièces : ";
            foreach(var f in Project.Floors)
            {
                foreach(var r in f.Rooms)
                {
                    res += r.Name + ", ";
                }
            }
            res += "\n";
            res += "Aides techniques insérées : \n";
            foreach(var f in Project.Floors)
            {
                foreach(var ff in f.Furnitures)
                {
                    if (ff.Type == "AideTechnique")
                    {
                        res += "- " + ff.Name + "\n";
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Load an autosave for the cancel / redo system
        /// </summary>
        /// <param name="auto"></param>
        public void LoadAutoSave(AutoSave auto)
        {
            string jsonString = File.ReadAllText(System.IO.Path.Combine(m_folderAutoSave, auto.ProjectPath));

            SelectedObjectManager.Instance.ResetSelection();

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ColorConverter());
            settings.Converters.Add(new QuaternionConverter());
            Project = JsonConvert.DeserializeObject<Project>(jsonString, settings);

            LoadFromCurrentFloor();

            ProjectFormScript.Instance.LoadProject();
        }

        /// <summary>
        /// Loads a json file into Project
        /// </summary>
        /// <returns></returns>
        public bool LoadProjectForReal()
        {
            filePath = FileBrowser.OpenSingleFile("json");
            if (filePath == "") return false;
            string jsonString = File.ReadAllText(filePath);

            SelectedObjectManager.Instance.ResetSelection();

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ColorConverter());
            settings.Converters.Add(new QuaternionConverter());
            Project = JsonConvert.DeserializeObject<Project>(jsonString, settings);

            LoadFromCurrentFloor();

            ProjectFormScript.Instance.LoadProject();
            newpopin.gameObject.SetActive(false);
            m_currentFilePath = filePath;

            OperationsBufferScript.Instance.ClearAutoSave();

            return true;
        }

        /// <summary>
        /// Set all elements of all creators and rebuild all scene data
        /// </summary>
        public void LoadFromCurrentFloor()
        {
            DestroyEverything();
            WallsCreator.Instance.LoadRoomsAndWallsFromFloor(Project.GetCurrentFloor());

            WallsCreator.Instance.wallHeight = Project.WallHeight;
            WallsCreator.Instance.wallThickness = Project.WallThickness;

            FurnitureCreator.Instance.LoadFurnituresFromFloor(Project.GetCurrentFloor());

            StairsCreator.Instance.LoadStairsFromFloor(Project.GetCurrentFloor());

            HelpersCreator.Instance.LoadHelpersFromFloor(Project.GetCurrentFloor());

            CharactersCreator.Instance.LoadCharactersFromFloor(Project.GetCurrentFloor());

            FloorPropertiesScript.Instance.SetCurrentFloor(ProjectManager.Instance.Project.CurrentFloorIdx);

            // GROUPS ALWAYS AT THE END
            GroupsManager.Instance.LoadGroupsFromFloor(Project.GetCurrentFloor());
        }

        /// <summary>
        /// SaveProject without checking if data would be lost
        /// </summary>
        public void SaveProjectNoCheck()
        {
            SaveProject();
        }

        /// <summary>
        /// Save without asking for name/path
        /// </summary>
        public void SaveCurrentFile()
        {
            SaveCurrentFloor();
            if (SaveProject(m_currentFilePath))
            {
                UIManager.Instance.ShowSaveOK();
            }
        }

        /// <summary>
        /// Save as (user defines path/name)
        /// </summary>
        public void SaveCurrentFileAs()
        {
            SaveCurrentFloor();
            if (SaveProject(""))
            {
                UIManager.Instance.ShowSaveOK();
            }
        }

        /// <summary>
        /// Create new autosave EVERYTIME an action is made on project
        /// Currently, autosaves are only used for cancel/redo system
        /// </summary>
        /// <returns></returns>
        public AutoSave SaveCurrentFileAsAutoSave()
        {
            SaveCurrentFloor();
            DateTime dt = DateTime.Now;

            if (!System.IO.Directory.Exists(m_folderAutoSave))
                System.IO.Directory.CreateDirectory(m_folderAutoSave);

            string path = "AUTOSAVE_" + String.Format("{0:yyyy_MM_dd_HH_mm_ss_ffff}", dt);

            string finalPath = System.IO.Path.Combine(m_folderAutoSave, path);

            if (SaveProject(finalPath, true))
            {
                return new AutoSave
                {
                    Date = dt,
                    ProjectPath = path
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Save project in json format at path
        /// </summary>
        /// <param name="path">file path</param>
        /// <param name="isAuto">is auto save for cancel redo ?</param>
        /// <returns></returns>
        public bool SaveProject(string path = "", bool isAuto = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = FileBrowser.SaveFile(Project.ProjectName, "json");
            }
            if (string.IsNullOrEmpty(path)) return false;

            if (!isAuto)
            {
                m_currentFilePath = path;
            }

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ColorConverter());
            settings.Converters.Add(new QuaternionConverter());

            SaveCurrentFloor();

            string jsonString = JsonConvert.SerializeObject(Project, settings);
            File.WriteAllText(path, jsonString);

            return true;
        }

        /// <summary>
        /// Copy data (make new references) and stores them in Project
        /// </summary>
        public void SaveCurrentFloor()
        {
            AllElementsManager.Instance.UpdateAllElements(true);
            Project.GetCurrentFloor().ClearAll();
            List<Room> wcRooms = WallsCreator.Instance.GetRooms();

            // Copy of rooms, walls, wallopenings
            foreach (var r in wcRooms)
            {
                Room newRoom = r.GetCopy() as Room;
                Project.GetCurrentFloor().Rooms.Add(newRoom);
            }
            // WALLS
            List<Wall> lonelyWalls = new List<Wall>();
            Wall[] allWalls = new Wall[WallsCreator.Instance.GetWalls().Count];
            WallsCreator.Instance.GetWalls().CopyTo(allWalls);
            // WALLS
            foreach (var w in allWalls)
            {
                bool isLonely = true;
                foreach (var r in Project.GetCurrentFloor().Rooms)
                {
                    if (r.Walls.Where(rw => rw.P1 == w.P1 && rw.P2 == w.P2).Count() == 1)
                    {
                        isLonely = false;
                    }
                }
                if (isLonely)
                {
                    Wall newRefWall = w.GetCopy() as Wall;
                    lonelyWalls.Add(newRefWall);
                }
            }
            Project.GetCurrentFloor().Walls = lonelyWalls;
            // FURNITURES
            List<Furniture> newFref = new List<Furniture>();
            foreach (var f in FurnitureCreator.Instance.GetFurnitures())
            {
                Furniture fu = f.GetCopy() as Furniture;
                newFref.Add(fu);
            }

            Project.GetCurrentFloor().Furnitures = newFref;

            // CUSTOMIZED STAIRS
            List<Stairs> newStairsRef = new List<Stairs>();
            foreach (var s in StairsCreator.Instance.GetStairs())
            {
                newStairsRef.Add(s.GetCopy() as Stairs);
            }
            Project.GetCurrentFloor().Stairs = newStairsRef;

            // TEXT ZONES
            List<TextZoneElement> newTextZoneRefs = new List<TextZoneElement>();
            foreach (var h in HelpersCreator.Instance.GetHelpers())
            {
                if (h is TextZoneElement)
                {
                    var oldT = (h as TextZoneElement);
                    newTextZoneRefs.Add(oldT.GetCopy() as TextZoneElement);
                }
            }

            Project.GetCurrentFloor().TextZoneElements = newTextZoneRefs;

            // CHARACTERS
            List<CharacterElement> newCharRefs = new List<CharacterElement>();
            foreach (var ce in CharactersCreator.Instance.GetCharacters())
            {
                newCharRefs.Add(ce.GetCopy() as CharacterElement);
            }

            Project.GetCurrentFloor().Characters = newCharRefs;

            // IMPORTANT : LET GROUPS AT THE END
            // GROUPS : SAVE ONLY IDS
            List<ElementGroup> newGroupsRefs = new List<ElementGroup>();
            foreach (var eg in GroupsManager.Instance.GetGroups())
            {
                var newIds = new List<int>();
                newIds.AddRange(eg.ElementsIds);
                newGroupsRefs.Add(new ElementGroup
                {
                    Id = eg.Id,
                    ElementsIds = newIds
                });
            }

            Project.GetCurrentFloor().Groups = newGroupsRefs;

            // PROPERTIES
            Project.WallHeight = WallsCreator.Instance.wallHeight;
            Project.WallThickness = WallsCreator.Instance.wallThickness;
        }

        /// <summary>
        /// Destroy current floor and load another one
        /// </summary>
        /// <param name="v">floor id</param>
        public void SetCurrentFloor(int v)
        {
            SaveCurrentFloor();
            Debug.Log("SET CURRENT FLOOR " + v);
            Project.CurrentFloorIdx = v;
            DestroyEverything();
            LoadFromCurrentFloor();
        }

        /// <summary>
        /// Creates a new floor, and set current floor to it
        /// </summary>
        /// <param name="floorName">Name</param>
        /// <param name="hasToCopyOtherFloor">Copy walls of another floor ?</param>
        /// <param name="copyName">Name of the floor you want to copy the walls</param>
        /// <param name="wheight">Wall height</param>
        public void AddFloor(string floorName, bool hasToCopyOtherFloor, string copyName, float wheight)
        {
            SetCurrentFloor(Project.Floors.IndexOf(Project.Floors.Where(fl => fl.FloorName == copyName).First()));
            ErgoShop.POCO.Floor newFloor = new ErgoShop.POCO.Floor
            {
                FloorName = floorName,
                WallHeight = wheight
            };

            if (hasToCopyOtherFloor)
            {
                ErgoShop.POCO.Floor floorCopy = Project.Floors.First(f => f.FloorName == copyName);
                foreach (var r in floorCopy.Rooms)
                {
                    foreach (var w in r.Walls)
                    {
                        Wall copy = new Wall
                        {
                            Color = w.Color,
                            Height = wheight / 100f,
                            P1 = w.P1,
                            P2 = w.P2,
                            Thickness = w.Thickness,
                            Index = w.Index
                        };
                        if (newFloor.Walls.Where(wa => wa.P1 == copy.P1 && wa.P2 == copy.P2).Count() == 0)
                        {
                            newFloor.Walls.Add(copy);
                        }
                    }
                }
                foreach (var w in floorCopy.Walls)
                {
                    Wall copy = new Wall
                    {
                        Color = w.Color,
                        Height = wheight / 100f,
                        P1 = w.P1,
                        P2 = w.P2,
                        Thickness = w.Thickness,
                        Index = w.Index
                    };
                    if (newFloor.Walls.Where(wa => wa.P1 == copy.P1 && wa.P2 == copy.P2).Count() == 0)
                    {
                        newFloor.Walls.Add(copy);
                    }
                }
            }

            Project.Floors.Add(newFloor);
            FloorPropertiesScript.Instance.AddFloorToDD(floorName);
            SetCurrentFloor(Project.Floors.Count - 1);
        }

        /// <summary>
        /// Ask every creator to delete its data and gameobjects
        /// </summary>
        public void DestroyEverything()
        {
            WallsCreator.Instance.DestroyEverything();
            FurnitureCreator.Instance.DestroyEverything();
            StairsCreator.Instance.DestroyEverything();
            HelpersCreator.Instance.DestroyEverything();
            CharactersCreator.Instance.DestroyEverything();
            GroupsManager.Instance.DestroyEverything();
        }

        /// <summary>
        /// Path to %appdata%/ErgoShop/Autosaves
        /// </summary>
        /// <returns></returns>
        public string GetAutoSaveFolderPath()
        {
            return m_folderAutoSave;
        }
    }
}