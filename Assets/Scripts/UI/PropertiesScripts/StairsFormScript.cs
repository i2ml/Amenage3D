using ErgoShop.Managers;
using ErgoShop.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    ///     UI Properties for selected element
    /// </summary>
    public class StairsFormScript : PropertiesBehaviour
    {
        public Slider nbStepsSlider, widthSlider, heightSlider, depthSlider, curvatureSlider, innerRadiusSlider;
        public Toggle buildSidesToggle, toTheLeftToggle;

        private bool needsUpdate;

        // Start is called before the first frame update
        private void Start()
        {
            needsUpdate = true;
        }

        // Update is called once per frame
        private void Update()
        {
            if (SelectedObjectManager.Instance.currentStairs.Count == 0)
            {
                needsUpdate = true;
                Hide();
                return;
            }

            Show();

            if (needsUpdate)
            {
                Debug.Log("Updating stairs UI");
                var s = SelectedObjectManager.Instance.currentStairs[0];
                nbStepsSlider.value = s.NbSteps;
                nbStepsSlider.transform.parent.GetChild(1).GetComponent<InputField>().text = s.NbSteps + "";
                widthSlider.value = s.Width * 100f;
                widthSlider.transform.parent.GetChild(1).GetComponent<InputField>().text = s.Width * 100f + "";
                heightSlider.value = s.Height * 100f;
                heightSlider.transform.parent.GetChild(1).GetComponent<InputField>().text = s.Height * 100f + "";
                depthSlider.value = s.Depth * 100f;
                depthSlider.transform.parent.GetChild(1).GetComponent<InputField>().text = s.Depth * 100f + "";
                curvatureSlider.value = s.Curvature;
                curvatureSlider.transform.parent.GetChild(1).GetComponent<InputField>().text = s.Curvature + "";
                innerRadiusSlider.value = s.InnerRadius * 100f;
                innerRadiusSlider.transform.parent.GetChild(1).GetComponent<InputField>().text =
                    s.InnerRadius * 100f + "";
                buildSidesToggle.isOn = s.BuildSides;
                toTheLeftToggle.isOn = s.ToTheLeft;

                needsUpdate = false;
            }

            toTheLeftToggle.gameObject.SetActive(curvatureSlider.value > 0);
            depthSlider.transform.parent.gameObject.SetActive(curvatureSlider.value == 0);
            innerRadiusSlider.transform.parent.gameObject.SetActive(curvatureSlider.value > 0);
            base.Update();
        }

        private void Show()
        {
            for (var i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(true);
            GetComponent<Image>().enabled = true;
        }

        public override void Hide()
        {
            for (var i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(false);
            GetComponent<Image>().enabled = false;
        }

        public void SetCanMove(bool v)
        {
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.IsLocked = v);
            needsUpdate = true;
        }

        public void SetNBSteps(float nb)
        {
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.NbSteps = (int) nb);
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.RebuildSceneData());
        }

        public void SetWidth(float width)
        {
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.Width = width / 100f);
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.RebuildSceneData());
        }

        public void SetHeight(float height)
        {
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.Height = height / 100f);
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.RebuildSceneData());
        }

        public void SetDepth(float depth)
        {
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.Depth = depth / 100f);
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.RebuildSceneData());
        }

        public void SetAngle(float angle)
        {
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.Curvature = angle);
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.RebuildSceneData());
        }

        public void SetInnerRadius(float radius)
        {
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.InnerRadius = radius / 100f);
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.RebuildSceneData());
        }

        public void SetToTheLeft(bool v)
        {
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.ToTheLeft = v);
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.RebuildSceneData());
        }

        public void SetBuildSides(bool v)
        {
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.BuildSides = v);
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.RebuildSceneData());
        }

        public void SetRotation(float f)
        {
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.Rotation = f);
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.RebuildSceneData());
        }

        public void SetRotation(string s)
        {
            float res = 0;
            if (ParsingFunctions.ParseFloatCommaDot(s, out res)) SetRotation(res);
        }

        public void SetRotationSlider(float f)
        {
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.Rotation = f * 5f);
            SelectedObjectManager.Instance.currentStairs.ForEach(s => s.RebuildSceneData());
        }
    }
}