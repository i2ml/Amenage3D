$(function() {
    registerTemplate('apis',`
        <h1 id="page-heading">API Reference</h1>
        <br/>
        <table class='mdl-data-table mdl-js-data-table'>
            <thead>
                <tr>
                    <th class='mdl-data-table__cell--non-numeric'>Name</th>
                    <th class='mdl-data-table__cell--non-numeric'>Summary</th>
                </tr>
            </thead>
            {{#forEachRootItem}}
                <tr>
                    <td class='mdl-data-table__cell--non-numeric lbl'><a page='{{signature}}'>{{name}}</a></td>
                    <td class='mdl-data-table__cell--non-numeric desc'>{{findComment this 'summary'}}</td>
                </tr>
            {{/forEachRootItem}}
        </table>
    `);
});