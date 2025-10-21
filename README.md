

# [PDF based HTML data entry](https://github.com/gstolarov/pdf-form)

## Introduction
There is quite a number of times where instead of the project I get handed a PDF file and asked to build a system to collect relevant data. Building a HTML form is not hard, but organizing input elements so it make sense to the end-user, documenting each input element, training users what to put where... is always a pain. The ideal situation is to collect data directly into PDF, but this is semi-proprietary mechanism, dealing with binary files, ... so much pain.

So I tried to see what options would be available. In a way I want a PDF as a background image with HTML input elements properly positioned on top. This is what got me started on my research:
 - convert PDF to image (SVG) 
 - save PDF input fields as an XML document, preferably inside the same SVG file to keep it together
 - When HTML page is rendered extract field positions from SVG file, find corresponding HTML input elements and position them appropriately.

 So the solution contains 2 different projects:
 - Convert PDF to a set of SVGs images with added instruction on input element positioning. 
 - Sample page to render SVG and properly place input elements

## PDF to SVG conversion.
While there is a number of utilities to convert PDF to SVG, I settled on using [InkScape](https://inkscape.org/). You need to download and install 64bit version as a pre-requisite - the app will split PDF file into separate pages and then will use following command line to convert:

    %ProgramFiles%\Inkscape\bin\inkscape.com -o ${filename}.svg ${filename}.pdf

Apparently SVG specs provide an element **svg/metadata** which is actually ignored by browser and can be used as needed to specify additional information. So I decided it's a good spot to tack PDF input elements in here. I use somewhat dated iTextSharp to extract data from the relevant PDF page and create a set of elements **svg/metadata/fields/input**. Since the SVG is just an XML file, it makes it easy to extract this information on the client/browser.

To run conversion process, run the utility, select PDF file and click on PDF->SVG button. If everything is OK, in the same folder as PDF file, it will create ${fileName}_PageXXX.svg files.

## HTML presentation
The second project is **Web**. All the logic is in the **index.html** file. The only 2 dependencies are the bootstrap to quickly draw tab control and jQuery. Both are included from CDN. 

#### CSS classes:
First let's define couple of CSS classes:
|Class | Description|
|--|--|
|.pdf-page |Designates a page container and sets it's position as relative so subsequent input elements can be absolute-positioned|
|.pdf-field | Automatically assigned to all the input elements w/i page container. Sets input elements absolute positioning, 0-margin, z-index|
|.pdf-page input, .pdf-page select| Sets a font size. Might need to be adjusted as needed for a different form or per elements. Also initially hides elements to prevent annoying flicker.|
|.pdf-page input[type="radio"], .pdf-page input[type="checkbox"] | Resets bootstrap setting margins on the check/radio boxes|
|.fade:not(.active)| Fix show/hide panels for bootstrap tab-bar|
|.invPrint|Make sure tab-bar is not printed|

#### Tab/navigation bar
Next step let's define a bootstrap tab control on the top of the page that would allow us to switch between the pages. You might have some other means of navigating between pages.

	<ul class="nav nav-tabs invPrint" style="padding-left:20px">
		<li class="nav-item"><a class="nav-link active" data-toggle="tab" data-target="#p1">
				Page 1</a></li>
		<li class="nav-item"><a class="nav-link" data-toggle="tab" data-target="#p2">
				Page 2</a></li>
	</ul>

So here **data-target** attribute designates an id of the elements that will be activated when tab is clicked with Page1 being active by default.

#### Pages
After that let's create a page containers:

    <div id="p1" class="pdf-page tab-pane fade active in" role="tabpanel"
		 aria-labelledby="page1-tab" img="f1040_Page1.svg" zoom=135>
	</div>
	<div id="p2" class="pdf-page tab-pane fade" role="tabpanel"
		 aria-labelledby="page2-tab" img="f1040_Page2.svg" zoom=135>
	</div>
	
As you can see div's id matches data-target attribute. I usually include additional action tabs in  the tab-bar such as save, delete, audit-trail, Each container has **pdf-page** class, that will be used to find those when page is loaded. Page 1 is active by default. Also there is 2 additional custom attributes on each page:
|attribute | Description  |
|--|--|
|img | Name/URL of the SVG file for this page |
|zoom | scale factor for a page - optional, default 100 |

#### Input fields
The easiest way to add fields is to open corresponding SVG file in a text editor, move to the **svg/metadata/fields** section copy/paste all the input elements into the appropriate container, and remove all the positional (top/left/width/height) and namespace attributes. Since field names/ids in the different pages might be duplicated, the code assume that all the field id should be prefixed with page/container id. The result page should look like:

    <div id="p1" class="pdf-page tab-pane fade active in" role="tabpanel"
		 aria-labelledby="page1-tab" img="f1040_Page1.svg" zoom=135>
		<input type="text" id="p1_f1" />
		<input type="text" id="p1_f2" />
		<input type="text" id="p1_f3" />
		...
	</div>
After that you need to go over each field and assigned appropriate name (handle bi-directional binding), check the type of the input element - possibly change from checkbox to radio or from input to select, provide options and checkbox values, validation, etc.

#### Javascript
Upon page load I use jQuery to initialize each page:

    $(document).ready(function () {
    	$('.pdf-page').each(function (i, el) { InitSvgPage($(el)); })
    });
    function InitSvgPage(page) {
    	var url = page.attr('img'), pid = page.attr('id'),
    		zoom = parseInt(page.attr('zoom') || 100);				/* 1 */
    	if (zoom > 1) 											
    		page.css({ transform: 									/* 2 */
	    		`translate(${(zoom-100)/2}%, ${(zoom-100)/2}%) scale(${zoom/100})` });
    	fetch(url).then(rsp => rsp.text()).then(function (html) {	/* 3 */
    		var svg = $(html).filter('svg'), coll = $('fields', svg), flds = $('input', coll);
    		var scl = parseFloat(svg.attr("height")) / parseFloat(svg.attr("width")),
    			cw = parseFloat(coll.attr("width"));
    		if (!scl || !cw) return;
    		$(`<img src="${url}" />`).prependTo(page).add(page)		/* 4 */
    			.css({ width: cw + 'px', height: (cw * scl) + 'px' })
    		flds.each(function (i, def) {
    			def = $(def);
    			var el = $(`#${pid}_${def.attr('id')}`, page);		/* 5 */
    			if (!el.length) return;
    			['left', 'top', 'width', 'height'].forEach(
		   				x => el.css(x, (parseFloat(el[0].style[x] || def.attr(x))) + 'px'))
    			el.addClass('pdf-field')
    		});
    	});
    }

1. retrieve SVG URL, page id and zoom/scale factors 
2. set page zoom/scale
3. retrieve actual SVG, convert it to DOM using jQuery and find all the svg/metadata/field/input elements.
4. add SVG as an image to the page container.
5. For each of the input elements from the SVG find appropriate HTML input element based on id (\${page_id}_\${field_id}) and set it's position/size/class


As an example I use 1040 US tax form PDF file. Please note that SVG files are created in step 1 - **PDF to SVG conversion**. I never change SVG file, since when there is change I likely would receive another PDF and SVG file would need to be regenerated. While all the input elements are added from the SVG file, I only changed some with correct name/type - in real life all the fields would need to be touched. If for some reason the field is not relevant, then just remove/comment-out it from page container.
In real life, you would also need to provide bi-directional binding and validation, but this is framework dependent and outside of the scope of the project. In my projects each field also have additional attribute specifying a binding information but like I said this depends on the framework information and would vary between Angular, React, ...

