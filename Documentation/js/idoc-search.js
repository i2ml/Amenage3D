let filters = [];
let dialog;

function search() {
	const input = document.getElementById('searchinput');
	const query = input.value;
	if (!query)
		return;
		
	const title = dialog.querySelector(".mdl-dialog__title");
	const content = $("#search-result");
	let html = "";
	const result = MEMBERS.filter(it => {
		return filters.includes(it.type) && it.name.toLowerCase().includes(query.toLowerCase());
	});

	const resultCount = result.length;
	for(const member of result){
		html +=  `<tr>
					<td>${member.type}</td>
					<td><a page="${member.signature}">${escape_html(member.signature)}</a></td>
				  </tr>
				`;
	}
	title.innerHTML = `Your search for ${query} resulted in ${resultCount}  matche(s) :`;
	content.html(html);	
	if (!dialog.open) {
		dialog.showModal();
	}
	process_links(content);
}

function filter_search(checkboxId) {
	const checkbox = dialog.querySelector("#"+checkboxId);
	const type = checkboxId.replace("checkbox-", '');
	if (checkbox.checked)
		filters.push(type);
	else
		filters = filters.filter(it => it !== type);

	search();
}

$(function() {
	dialog = document.querySelector('dialog');
	dialogPolyfill.registerDialog(dialog);
	dialog.querySelector('.close').addEventListener('click', function() { dialog.close();});
	
	let html = '';
	for (const type of MEMBER_TYPES) {
		filters.push(type);
		html += `<label class="mdl-checkbox mdl-js-checkbox mdl-js-ripple-effect" for="checkbox-${type}">
					<input type="checkbox" id="checkbox-${type}" class="mdl-checkbox__input" checked onclick="filter_search('checkbox-${type}')">
					<span class="mdl-checkbox__label">${type}</span>
				 </label>`;
	}
	
	$('#search-filters__content').html(html);

	componentHandler.upgradeDom('#search-filters__content');
	
	$('#searchbtn').click(search);

	$('#searchinput').keyup(function(e) {
		const code = e.which;
		if (code === 13) {
			search();
		}
	});
});