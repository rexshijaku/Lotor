# Lotor

Lotor - is a hybrid type of focused and incremental crawlers: (1) focused since it collects relevant (Albanian)
documents and (2) incremental because it incrementally refreshes the existing collection of a domain list. This language focused web crawler aims to exclude domains which are not written in Albanian from a given set of domains and ranks exclusively Albanian domains by their importance. This tool makes predictions about the size of domains, the domains main language (as Albanian or not), the multilingual domains, where at least one of the languages is Albanian, and (4) the quality of a domain. 

Lotor - gets all its information for the domain by crawling maximum its thre levels.

Procyon lotor is a mammal who dips his food in water before eating, and Lotor from latin stands for washer. Our crawler doesn't decide that a domain is Albanian just by its index page language, but it analyzes its index page and all first level pages.

Usage : 

Clone this repository 

Make sure all packages are installed.

You will need to run on Packer Manager Console following command: 

Install-Package HtmlAgilityPack

Helpful information: 

Lotor/lotor_input and Lotor/lotor_output are the only folders we should focus on.

Lotor/lotor_input folder contains all important files which are essential for Lotor.

Domain urls you want to crawl should be added in seed.txt on Lotor/lotor_input folder in the following format: startek.al, shqipfm.al, linktone.al which should be separated in lines.

Files such as al_stopwords.txt and albTerms4.txt are not complete (due to copyright reasons)! 
Whenever you need these files, I can email  you back! Write me on rexhepshijaku@gmail.com

More information about Lotor/lotor_input folder you can find in Globals/Configs.cs commented lines.

In Lotor/lotor_output are created temporary (cached) files and those which sum up the crawl process. 
For example folder results contains files such : albanian_domains.csv, nonalbanian_domains.csv, likely_albanian_domains.html and likely_multilingual.htm  which give information about the domains previously crawled.
