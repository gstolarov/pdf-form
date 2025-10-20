
# [PDF based HTML data entry](https://github.com/gstolarov/sql-server-geocoding)

## Introduction
There is quite a number of times where instead of the project I get handed a PDF file and asked to build a system to collect relevant data. Building a HTML form is not hard, but organizing input elements so it make sense to the end-user, documenting each input element is always a pain. The ideal situation is to collect data directly into PDF, but this is semi-proprietary mechanism, dealing with binary files, ... so much pain.

So I tried to see what options would be available. In a way I want a PDF as a background image with HTML input elements properly positioned on top. This is what got me started on my research:
 - convert PDF to image (SVG) 
 - save PDF input fields as an XML document, preferably inside the same SVG file to keep it together
 - When HTML page is rendered extract field positions from SVG file, find appropriate HTML input elements and position them appropriately.

 So the solution contains 2 different projects:
 - Convert PDF to a set of SVGs  HTML 
 - app to render SVG and properly place input elements

## PDF to SVG conversion.
While there is a number of utilities to convert PDF to SVG, I settled on using [InkScape](https://inkscape.org/). You need to download and install 64bit version as a pre-requisite - the app will use following command line to convert:

    %ProgramFiles%\Inkscape\bin\inkscape.com -o ${filename}.svg ${filename}.pdf

Apparently SVG specs provide an element **svg/metadata** which is actually ignored by browser and can be used as needed to specify additional information. So I decided it's a good spot to tack PDF input elements in here. I use somewhat dated iTextSharp to extract data from the relevant PDF page and create a set of elements **svg/metadata/fields/input**. Since the SVG is just an XML file, it makes it easy to extract this information on the client/browser.

To run conversion process, run the utility, select PDF file and click on PDF->SVG button. If everything is OK, in the same folder as PDF file, it will create ${fileName}_PageXXX.svg files.

## HTML presentation
The second project is **Web**. All the logic is in the **index.html** file. The only 2 dependencies are the bootstrap to quickly draw tab control and jQuery. Both are included from CDN. 
Upon page load (**jQuery.ready** function), for each PDF Page (a div designated with **.pdf-page** class, it will execute **InitSvgPage** function, which will:
 - load SVG file pointed to by **img** attribute
 - get page id (**id** attribute)
 - load scaling factor (optional) based on **zoom** attribute and scale page based on this attribute
 - load all input elements from SVG file (**svg/metadata/fields/input**)
 - For each input element in the **svg/metadata/fields/input** list, find input element in the page div with id of **\${page_id}_\${svg_fld_id}** id and position it based on SVG specifications (extracted from PDF), and add **pdf-formfld** class.

As an example I use 1040 US tax form PDF file. I got lazy and added only couple of fields just as provide proof-of-concept. To add more fields, just copy relevant field from SVG file, add page_id prefix to the input field attribute and remove positional (left, right, height, width) attributes - element will be positioned by **InitSvgPage** function. If for some reason the field is not relevant, then just not add corresponding input element.
In real life, you would also need to provide bi-directional binding and validation, but this is framework dependent and beside the scope of the project. In my projects each field also have additional attribute specifying a binding information but like I said this depends on the framework information and would vary between Angular, React, ...