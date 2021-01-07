$(function() {
    registerTemplate('default',`
        <h1 id="page-heading">{{name}}</h1>
        {{#if scope}}
            <span>
                <i class="fas fa-angle-double-left"></i>
                <a page='{{scope}}'>{{scope}}</a> <i class="fas fa-angle-double-right"></i>
            </span>
        {{/if}}

        {{#ifHasComment tag='summary'}}
            <section class='section'>
                <p id="summary">{{{findComment this 'summary'}}}</p>
            </section>
        {{/ifHasComment}}

        <section class='section'>
            <span class='section-heading'>Syntax</span>
            {{processCode token}}
        </section>
       
        {{#ifHasComment tag='typeparam'}}
            <section class='section'>
                <span class='section-subheading'>TypeParams</span>
                {{#forEachComment tag='typeparam'}}
                    <div class='flex'>
                        <span class='paramname'>{{name}}</span>
                    </div>
                    <p class='paramdesc'>{{{comment}}}</p>
                    <br/>
                {{/forEachComment}}
            </section>
        {{/ifHasComment}}

        {{#if params}}
            <section class='section'>
                <span class='section-subheading'>Parameters</span>
                {{#forEachParam}}
                    <div class='flex'>
                        <span class='paramname'>{{name}}</span>
                        <span class='paramtype'>{{type}}</span>
                    </div>
                    <p class='paramdesc'>{{{comment}}}</p>
                    <br/>
                {{/forEachParam}}
            </section>
        {{/if}}
        
        {{#ifHasComment tag='returns'}}
            <section class='section'>
                <span class='section-subheading'>Returns</span>
                <p class='paramdesc'>{{{findComment this 'returns'}}}</p>
                <br/>
            </section>
        {{/ifHasComment}}

        {{#ifHasComment tag='exception'}}
            <section class='section'>
                <span class='section-subheading'>Exceptions</span>
                {{#forEachComment tag='exception'}}
                    <div class='flex'>
                        <span class='paramname'>{{name}}</span>
                    </div>
                    <p class='paramdesc'>{{{comment}}}</p>
                    <br/>
                {{/forEachComment}}
            </section>
        {{/ifHasComment}}

        
        {{#ifHasOverload}}
            <section class='section'>
                <span class='section-heading'>Overloads</span>
                <table class='mdl-data-table mdl-js-data-table'>
                    <thead>
                        <tr>
                            <th class='mdl-data-table__cell--non-numeric'>Name</th>
                            <th class='mdl-data-table__cell--non-numeric'>Summary</th>
                        </tr>
                    </thead>
            
                    {{#forEachOverload}}
                        <tr>
                            <td class='mdl-data-table__cell--non-numeric lbl'><a page='{{this.signature}}'>{{this.name}}</a></td>
                            <td class='mdl-data-table__cell--non-numeric desc'>{{findComment this 'summary'}}</td>
                        </tr>
                    {{/forEachOverload}}
                </table>
            </section>
        {{/ifHasOverload}}

        {{#ifHasComment tag='remarks'}}
            <section class='section'>
                <span class='section-heading'>Remarks</span>
                <div id="remarks">{{{findComment this 'remarks'}}}</div><br/>
            </section>
        {{/ifHasComment}}
    
        {{#ifHasComment tag='example'}}
            <section class='section'>
                <span class='section-heading'>Example</span>
                <div id="example">{{{findComment this 'example'}}}</div><br/>
            </section>
        {{/ifHasComment}}

        {{#forEachChildItems}}
            <section class='section'>
                <span class='section-heading'>{{sectionName type}}</span>
                <table class='mdl-data-table mdl-js-data-table'>
                <thead>
                    <tr>
                        <th class='mdl-data-table__cell--non-numeric'>Name</th>
                        <th class='mdl-data-table__cell--non-numeric'>Summary</th>
                    </tr>
                </thead>
                    {{#each childs}}
                        <tr>
                            <td class='mdl-data-table__cell--non-numeric lbl'><a page='{{this.signature}}'>{{this.name}}</a></td>
                            <td class='mdl-data-table__cell--non-numeric desc'>{{findComment this 'summary'}}</td>
                        </tr>
                    {{/each}}
                </table>
            </section>
        {{/forEachChildItems}}
    `);
});