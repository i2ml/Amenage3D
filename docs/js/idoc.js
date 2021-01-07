// Author: Mamadou Cisse
// mciissee@gmail.com
function compare(a, b) {
	a = a.signature;
	b = b.signature;
	if (a.length != b.length) {
	 	return a.length - b.length;
	}
	return (a < b) ? -1 : (a > b) ? 1 : 0;
}

ROOT_MEMBERS.sort(compare);
MEMBERS.sort(compare);

const MEMBER_TYPES = [
	'Namespace',
	'Package',
	'Header',
	'Class',
	'Struct',
	'Union',
	'Interface',
	'Enum',
	'Field',
	'Property',
	'Constructor',
	'Method',
	'Indexer',
	'Operator',
	'EnumValue',
	'Macro'
];

const SECTION_NAMES = {
	Namespace: 'Namespaces',
	Class: 'Classes',
	Struct: 'Structures',
	Interface: 'Interfaces',
	Enum: 'Enumerations',
	Field: 'Fields',
	Property: 'Properties',
	Constructor: 'Constructors',
	Method: 'Methods',
	Indexer: 'Indexers',
	Operator: 'Operators',
	EnumValue: 'EnumValues',
	Macro: 'Macros',
	Union: 'Unions',
	Package: 'Packages',
	Header: 'Headers'
};

const SEE_REGEX = /<see\s+cref=(?:"|')([^'"]+)(?:"|')\s*\/?>/g;
const CODE_REGEX = /<code>((?:.|\s)*?)<\/code>/g;
const STRING_REGEX = /(?:(?:[\@\$]*"(?:(?:\\")|(?:\\\\)|\s|[^"])*")|(?:'(?:(?:\\'[^'])|\s|[^']{1,2})'))/g;
const COMMENT_REGEX = /(?:(?:\/\*(?:[^*]|(?:\*+[^*\/]))*\*+\/)|(?:\s*\/\/[^\n]*))+/g;
const PARAM_REF_REGEX = /(?:<paramref\s+name=)(?:"|')(\w+)(?:"|')\s*\/>/g;
const CODE_WORD_REGEX = /<c>((?:.|\s)*?)<\/c>/g;
const TYPE_PARAM_REF_REGEX = /(?:<typeparamref\s+name=)(?:"|')(\w+)(?:"|')\s*\/>/g;
const FUNCTIONS_NAME_REGEX = /\b\w+(?=\s*\()/g;

var TEMPLATES = {};
var GUIDES = {};

function registerTemplate(template, script) {
    TEMPLATES[template] = script;
}

function registerGuide(guide) {
	GUIDES[guide.name] = guide;
}

function compileTemplate(template, context) {
    let script = TEMPLATES[template];
    if (typeof script === 'string') {
        script =  Handlebars.compile(script);
        TEMPLATES[template] = script;
    }
    return script(context);
}

/**
 * process the elements with the class '.accordion' 
 * inside of the given dom element 
 * @param {*} element the dom element
 * @param {*} toggleState if sets to true the state 
 */
function process_accordions(element) {
	element = typeof element === 'string' ? $(element) : element;
	element.find(".accordion-toggle").click(function() {
		const toggle = $(this);
		let accordion = $(this).parent().children('.accordion');
		accordion.slideToggle('fast', function() {
			if (accordion.is(":visible")) {
				toggle.attr('class', 'fa fa-angle-down .accordion-toggle');
			}
			else {
				toggle.attr('class', 'fa fa-angle-right .accordion-toggle');
			}
		});
	});
	element.find('.accordion').slideToggle('fast');
}

/**
 * computes the string by replacing all occurences of < and > by &lt and &gt
 * @param {string} text the text to escape
 * @returns a string
 */
function escape_html(text) { 
	return text.replace(/</g, '&lt;').replace(/>/g, '&gt;'); 
}

/** 
 * add line numbers to <pre></pre> tags 
 * */
function process_code_lines() {
	const pre = document.getElementsByTagName('pre'),
	pl = pre.length;
    for (let i = 0; i < pl; i++) {
        pre[i].innerHTML = '<span class="line-number"></span>' + pre[i].innerHTML + '<span class="cl"></span>';
        let num = pre[i].innerHTML.split(/\n/).length;
        for (let j = 0; j < num; j++) {
            let line_num = pre[i].getElementsByTagName('span')[0];
            line_num.innerHTML += '<span>' + (j + 1) + '</span>';
        }
    }
}

/** 
 * finds the child members of the given item 
 * @param {*}  member the member
 * @returns an object of type {[x: string]: any;} where the key 'x' represents
 * the type of the members in the array 'any'
*/
function findChilds(member) {
	if (member.childcount === 0)
		return {};
	let childs = {};
	for (const type of MEMBER_TYPES) {
		const childItems = MEMBERS.filter(it => {
			return it.type == type && it.scope === member.signature;
		});
		if (childItems.length === 0)
			continue;
		
		childItems.sort(function(a, b) {
			a = a.signature;
			b = b.signature;
			return (a < b) ? -1 : (a > b) ? 1 : 0;
		});
		childs[type] = childItems;
	}
	return childs;
}

/**
 * finds and process all 'tag' comments of the member
 * @param {*} member the member
 * @param {*} tag the type of the tag
 * @returns an array of {name: string, comment: string} objects 
 * where the name is the value of attribute in the tag and comment is value of the tag.
 * 
 * @example with the tag <exception name='NullReferenceException'>...</exception> 
 * name is 'NullReferenceException' and comment '...'
 */
function findComments(member, tag) {
	if (member.comments) {
		let comments = [];
		for (const name of Object.keys(member.comments)) {
			if (name.startsWith(tag)) {
				comments.push({
					name: name.replace(tag + '-', ''),
					comment: process_comment(member, member.comments[name]),
				});
			}
		}
		return comments;
	}
	return [];
}

/**
 * finds and process all 'param' comments of the member
 * @param {*} member the member
 * @returns an array of {name: string, type: string, comment: string} objects 
 * where the name is the name of a parameter,
 * type the type of the parameter and comment the comment of the parameter
 */
function findParams(member) {
	if (member.params) {
		let params = [];
		let param = {};
		for (const name of Object.keys(member.params)) {
			param = {
				name: name,
				type: member.params[name]
			};
			if (member.comments) {
				param.comment = process_comment(member, member.comments['param-' + name]) || '';
			}
			params.push(param);
		}
		return params;
	}
	return [];
}

/**
 * finds and process the comment of type 'tag' of  the member
 * @param {*} member the member
 * @param {*} tag the type of the tag
 * @returns the value of the tag
 */
function findComment(member, tag) {
	if (member.comments) {
		return process_comment(member, member.comments[tag]) || '';
	}
	return '';
}

/**
 * checks if the member has an overloaded version
 * @param {*} member the member
 */
function hasOverload(member) {
	return MEMBERS.find(it => it.signature != member.signature && it.scope === member.scope && it.name === member.name);
}

/**
 * finds the overloads of the member
 * @param {*} member the member
 */
function findOverloads(member) {
	return MEMBERS.filter(it => it.signature != member.signature && it.scope === member.scope && it.name === member.name);
}

/**
 * checks if the member has a comment of the type 'tag'
 * @param {*} member the member
 * @param {*} tag the tag
 */
function hasComment(member, tag) {
	if (member.comments) {
		return Object.keys(member.comments).find(it => it.startsWith(tag));
	}
	return false;
}

/**
 * calls the function action for each child of the member.
 * @param {*} member the member
 * @param {*} action the function
 */
function forEachChildItems(member, action) {
	if (member.childcount === 0)
		return;
	const childs = findChilds(member);
	for (const type of Object.keys(childs))
		action(type, childs[type]);
}

/**
 * process the string 'code' by resolving the comments, string and language keywords
 * @param {*} code the string
 */
function process_code(code) {
	let i = 0;
	let coms = {};
	let strings = {};
	
	code = code.replace('<code>', '').replace('</code>', '');
	code = escape_html(code);
	// HIDE STRINGS
	code = code.replace(STRING_REGEX, function(match) {
		i++;
		strings['###' + i] = match;
		return '###' + i;
	});

	// HIDE COMMENTS
	code = code.replace(COMMENT_REGEX, function(match) {
		i++;
		coms['###' + i] = match;
		return '###' + i;
	});
	
	// PROCESS LANGAGE KEYWORDS
	code = code.replace(BUILTIN_WORDS_REGEX, '<span class="code--builtin">$&</span>');
	// PROCESS METHOD NAMES
	code = code.replace(FUNCTIONS_NAME_REGEX, '<span class="code--method">$&</span>');

	// SHOW AND PROCESS COMMENTS
	for (const k of Object.keys(coms))
		code = code.replace(k, `<span class="code--comment">${coms[k]}</span>`);
			
	// SHOW AND PROCESS STRINGS
	for (const k of Object.keys(strings))
		code = code.replace(k, `<span class="code--string">${strings[k]}</span>`);
	
	return `<pre class="code"><code>${code}</code></pre>`;
}

/**
 * process the string comment by resolving the special tags inside of it
 * like '<see cref>', '<code></code>'...
 * @param {*} member the member
 * @param {*} comment the comment
 */
function process_comment(member, comment) {
	if (comment) {	
		function process_param(match, $1) {
			if (!member)
				return match;			
			const param = member.comments['param-'+$1] ||  member.comments['typeparam-'+$1];
			if (param) {
				const id = `param-${$1}-${Math.random()*100}`;
				return `<span id="${id}" class="paramname">${$1}</span>
						<div class="mdl-tooltip mdl-tooltip--top" data-mdl-for="${id}">
							${process_comment(member, param)}
						</div>`;
			}
			return `<span class="paramname">${$1}</span>`;
		}

		function process_see(match, $1) {
			const cref = $1.replace(/\{/g, '<').replace(/}/g, '>');
			const item = MEMBERS.find(it => it.signature.endsWith(cref));
			if (item == undefined)
				return escape_html(cref);
			return `<a page="${item.signature}">${escape_html(cref)}</a>`;
		}
		return  comment.replace(TYPE_PARAM_REF_REGEX, process_param)
				   	   .replace(PARAM_REF_REGEX, process_param)
				   	   .replace(CODE_WORD_REGEX, `<span class="code--builtin">$1</span>`)
					   .replace(SEE_REGEX, process_see)
					   .replace(CODE_REGEX, process_code);
	}
	return comment;
}

/**
 * build the navigation tree of the page.
 */
function process_navigation() {
	let html = "<ul class='navtree'>";
	function open(linkName, signature, level) {
		html += "<li>";
		html += "<i class='fa fa-angle-right accordion-toggle'></i>";
		if (signature)	
			html += `<a class='navlink navlink--lvl${level}' page='${signature}'>${escape_html(linkName)}</a>`;
		else
			html += `<span class='navlink navlink--lvl${level}'>${escape_html(linkName)}</span>`;
		html += "<ul class='accordion'>";
		return html;
	}
	
	function close() {
		html += "</ul>";
		html += "</li>";
	}
	
	function build(member) {
		if (member.childcount === 0) {
			html += `<li>
						<a class='navlink navlink--lvl3' page='${member.signature}'>
							${escape_html(member.name)}
						</a>
					</li>`;
		} else {
			open(member.name, member.signature, 3);
			const childs = findChilds(member);
			for (const type of Object.keys(childs)) {
				open(SECTION_NAMES[type], null, 3);
				for (const child of childs[type])
					build(child);
				close();
			}
			close();
		}
	}
	
	for (const member of ROOT_MEMBERS) {
		build(member);
		html += "<hr/>";
	}	
	html += "</ul>";
	
	const navigation = $('#navigation');
	navigation.html(html);
	
	process_accordions(navigation);
	process_links(navigation);
}

/**
 * handles the clicks of the <a></a> elements inside of the given dom element
 * @param {*} element the dom element
 */
function process_links(element) {
	element.find('a[page]').on('click', e => {
		let page = e.currentTarget.getAttribute('page');
		window.location.hash = page;
	});
}

/**
 * render the page correspoding to the current url of the browser.
 */
function process_page() {
	const loadPage = function(page) {
		const main = $('#mdl-layout__content');
		main.html(page);
		process_accordions(main);
		process_links(main);
		process_code_lines();
		componentHandler.upgradeDom();
		$(".app-loading").fadeOut();
	}
	
	switch (window.location.hash) {
		case '':
		case '#apis':
			$("#apis").addClass("is-active");
			$("#guides").removeClass("is-active");
			loadPage(compileTemplate('apis'));
			break;
		case '#guides':
			$("#apis").removeClass("is-active");
			$("#guides").addClass("is-active");
			loadPage(compileTemplate('guides'));
			break;
		default:	
			const hash = unescape(window.location.hash.substring(1));
			const item = MEMBERS.find(it => it.signature == hash);
			if (item) {
				loadPage(compileTemplate('default', item));
			} else {
				const guide = GUIDES[hash];
				if (guide) {
					if (!guide.processed) {
						guide.content = process_comment(undefined, guide.content);
						guide.processed = true;
					}
					loadPage(Handlebars.compile(guide.content)());
				} else {
					loadPage(compileTemplate('error'));
				}
			}
			break;
	}
}

$(function() {
	Handlebars.registerHelper('forEachRootItem', function(options) {
		const rootItems = MEMBERS.filter(it => it.scope === '');
		let html = '';
		for (const it of rootItems)
			html += options.fn(it);
		return html;    
	});
	Handlebars.registerHelper('forEachChildItems', function(options) {
		const childs = findChilds(this);
		let html = '';
		for (const type of Object.keys(childs))
			html += options.fn({type: type, childs: childs[type]});
		return html;    
	});
	Handlebars.registerHelper('forEachOverload', function(options) {
		const overloads = findOverloads(this);
		let html = '';
		for (const it of overloads)
			html += options.fn(it);
		return html;    
	});
	Handlebars.registerHelper('forEachParam', function(options) {
		const params = findParams(this);
		let html = '';
		for (const param of params)
			html += options.fn(param);
		return html;   
	});
	Handlebars.registerHelper('forEachComment', function(options) {
		const tag = options.hash.tag;
		const comments = findComments(this, tag);
		let html = '';
		for (const it of comments)
			html += options.fn(it);
		return html;   
	});
	Handlebars.registerHelper('forEachGuide', function(options) {
		const guides = Object.keys(GUIDES).map(it => GUIDES[it]);
		let html = '';
		for (const it of guides)
			html += options.fn(it);
		return html;   
	});
	Handlebars.registerHelper('findComment', function(member, tag) {
		return new Handlebars.SafeString(findComment(member, tag));
	});
	Handlebars.registerHelper('processCode', function(code) {
		return new Handlebars.SafeString(process_code(code));
	});
	Handlebars.registerHelper('ifHasComment', function(options) {
		const tag = options.hash.tag;
		if (hasComment(this, tag)) {
			return options.fn(this);
		}
		return '';
	});
	Handlebars.registerHelper('ifHasOverload', function(options) {
		if (hasOverload(this)) {
			return options.fn(this);
		}
		return '';
	});
	Handlebars.registerHelper('ifEquals', function(arg1, arg2, options) {
		return (arg1 == arg2) ? options.fn(this) : options.inverse(this);
	});
	Handlebars.registerHelper('sectionName', function(type) {
		return SECTION_NAMES[type];
	});
	
	$(window).on('hashchange', process_page);

	process_links($('.mdl-layout__header'));
	process_navigation();
	process_page();
});