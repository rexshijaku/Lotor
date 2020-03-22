# Lotor

Lotor - wrriten in C#, is a hybrid type of focused and incremental crawlers: (1) focused since it collects relevant (Albanian)
documents and (2) incremental because it incrementally refreshes the existing collection of a domain list. This language focused web crawler aims to exclude domains which are not written in Albanian from a given set of domains and ranks exclusively Albanian domains by their importance. This tool makes predictions about the size of domains, the domains main language (as Albanian or not), the multilingual domains, where at least one of the languages is Albanian, and (4) the quality of a domain. Lotor gets all the necessary information for the domain by crawling maximum its three levels.

Procyon lotor is a mammal who dips his food in water before eating, and Lotor from Latin stands for washer. This web-crawler doesn't decide that a domain is Albanian just by checking its index page language, but it analyzes its index page and all first level pages, so it washes it well.

Configuration and usage : 
<ol>
  <li> Clone this repository </li>
  <li> Make sure all packages are installed </li>
  <li> You will need to run on Packer Manager Console following command: Install-Package HtmlAgilityPack </li>
</ol>

Alomst every piece of code is commented.

Helpful information: 

<b>Lotor/lotor_input</b> and <b>Lotor/lotor_output</b> are the only folders we should focus on.

<b>Lotor/lotor_input</b> folder contains all important files which are essential for Lotor to start its work.

Domain urls <u>you want to crawl</u> should be added in <b>seed.txt</b> which is located in <br>Lotor/lotor_input</b> folder in the following format:

<ul>
<li>nytimes.com</li>
<li>shqipfm.al</li>
<li>startek.al</li>
<li>linktone.al</li>
<li>eurosistemalbania.al</li>
<li>sabah.com.tr</li>
<li>emeraldhotel.info</li>
</ul>

the result of the preceding list processed by Lotor will be as below : 

<ul>
<li><strike>nytimes.com</strike></li>
<li>startek.al 41.583</li>
<li>shqipfm.al 14.212</li>
<li>eurosistemalbania.al 4.67</li>
<li>linktone.al 1.87</li>
<li><strike>sabah.com.tr</strike></li>
<li><strike>emeraldhotel.info</li></li>
</ul>

Files such as: <b>al_stopwords.txt</b> (which should contain 45 stopwords) and <b>albTerms4.txt</b> (which should contain 30000 the most common Albanian words on the web which contain at least four letters) are incomplete (due to copyright and company privacy)! 
Whenever you need these files, I can email you back, in minutes or hours! Write me in rexhepshijaku@gmail.com for these files and any other kind of help.

<b>Lotor/lotor_output</b> contains cache folders and descriptive files which sum up the crawl process.
For instance <b>results</b> folder contains files such : albanian_domains.csv, nonalbanian_domains.csv, likely_albanian_domains.html and likely_multilingual.htm  which give information about the domains recently crawled.

From the previous input list <b> albanian_domains.csv </b> at the end of crawling process should contain these domains : startek.al, shqipfm.al, eurosistemalbania.al and linktone.al, since these al are writen in Albanian. On the other hand <b>nonalbanian_domains.csv</b> should be populated by nytimes.com, sabah.com.tr and emeraldhotel.info because these domains have nothing to do with Albanian. File <b>likely_albanian_domains.html</b> will contain emeraldhotel.info because this domain is multilingual and it contains Albanian language as an Alternative language in this url in following format:

<ul>
  <li>emeraldhotel.info => emeraldhotel.info?lang=sq.</li>
</ul>

and similarly the file <b>likely_multilingual.htm</b> will contain startek.al, because it has more than one language in following format: 

<ul>
  <li>startek.al => startek.al/?lang=en</li>
</ul>

<br>Additional information about Lotor/lotor_input folder you can find in Globals/Configs.cs commented lines.</b>

Lotor was used to test our proposed methods in a scientific paper published as: "Model-based prediction of the size, the language and the quality of the web domains" and it produced highly accurate results in determining and classifying Albanian and non-Albanian domains.

This project can be generalized into more than one language.
