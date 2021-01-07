$(function() {
    registerTemplate('guides',`
        <h1 id="page-heading">Guides</h1> 
        <div class='mdl-grid'>
            {{#forEachGuide}}
                <div class='mdl-cell mdl-cell--4-col'> 
                    <div class='guide-card mdl-card mdl-shadow--2dp'> 
                        <div class='mdl-card__title mdl-card--expand'> 
                        <h2 class='mdl-card__title-text'>{{name}}</h2> 
                        </div> 
                        <div class='mdl-card__supporting-text'> 
                            {{#if summary}}
                                {{summary}}
                            {{/if}}
                        </div> 
                        <div class='mdl-card__actions mdl-card--border'> 
                            <a class='mdl-button mdl-button--colored mdl-js-button mdl-js-ripple-effect' page='{{name}}'> 
                                Learn 
                            </a> 
                        </div> 
                    </div> 
                </div>
            {{/forEachGuide}}
        </div>
    `);
});