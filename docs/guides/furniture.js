
// DO NOT FORGET TO ADD A REFERENCE OF YOUR GUIDE FILE IN THE INDEX.HTML FILE
$(function() {
    registerGuide({ 
        name: 'Notes', // THE NAME OF THE PAGE
        summary: 'Quelques notes concernant le code', // A SUMMARY DISPLAYED IN THE GUIDES PAGES
        content: `
            <h1 class='page-heading'>Quelques notes</h1>

            <section>
                <span class=‘section-heading'>Ajout de propriétés à un POCO :</span>
                <p>
					Gérer le GetCopy() en ajoutant la propriété en question, modifier le GetDescription()
				</p>
                
            </section>

            <section>
             <span class=‘section-heading'>Ajouter un mobilier au logiciel</span>
				<p>
					<ol>                                                                                                    
					<li>1 Prefabs : duppliquer un existant (Dans Furnitures 3D)                                              </li>
					<li>2 C/C dans resources le model 3d et ajouter rigidbody constraints et mesh collider	                 </li>
					<li>3 mettre à jour son nom dans le prefab                                                               </li>
					<li>4 Ajouter dans l'ui un bouton qui renvoi au prefab créé en (1)                                       </li>
					<li>5 lancer le programme, appuyer sur le bouton pour placer le meuble, et regarder les dimensions       </li>
					<li>6 créer l'asset 2D avec en dimensions le X et Z des dimensions indiquées                             </li>
					<li>7 mettre à jour le prefab (1) avec l'asset 2D                                                        </li>
					</ol>
				</p>
			</section>
		
			<section>
			<span class=‘section-heading'>Autres</span>
				<p>			
				Tous les managers sont des singleton avec une instance accessible aux autres classes
				</p>
				<p>
				De nombreux behaviours sont aussi des singleton, afin de combiner les scripts aux classes poco notamment
				</p>
				<p>
				Sauvegarde des parametres, autosaves et imports stockés dans le dossier %appdata%/ErgoShop
				<p>
				Pour faire la différence entre le projet à l'interieur du logiciel et le projet fabricant le logiciel j'utilise ces mots :<br/>
				Project : Les données d'un projet utilisateur au sein du logiciel<br/>
				Software : L'ensemble du code permettant le fonctionnement du logiciel<br/>
				</p>
			</p>
			</section>
		
        `
    });
});