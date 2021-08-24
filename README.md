# AudibleApi
Audible API in C#

Implementation of an Audible API in C#. Aside from the goal of having this be pure C#, the focus on this project was to have it

1) fully integratable into other software
2) fully testable and tested

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PalPal.me](https://paypal.me/mcrackan?locale.x=en_us)
...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.

Thanks to these other Audible APIs which I used for reference and whose authors were very responsive and helpful with questions:

https://github.com/omarroth/audible.cr -- Crystal interface for Audible's internal API  
https://github.com/mkb79/Audible -- Python interface for Audible's internal API

The main functionality of this API is typically kept very up to date. It is the backbone of my popular-ish audible library manager [Libation](https://github.com/rmcrackan/Libation). Although some tests and demo code are probably outdated by now, I'm happy to answer questions though in the [issues](https://github.com/rmcrackan/AudibleApi/issues) page.

## Getting started

Anything you do will start with a call to `AudibleApi.EzApiCreator.GetApiAsync`. This async method will return an instance of the api object which you will use for everything else. When login info is needed, it will be processed by the `ILoginCallback` class you specify.

AudibleApiClientExample provides some quick and dirty examples. It's what I use for quick tests so there are scraps of examples throughout.

`AudibleApiClientExample.LoginCallback` provides a proof of concept for a command line callback class.

`AudibleApiClientExample.AudibleApiClient` shows how to create the api object which is then used in `_Main.run`

## response_groups

Since the API is undocumented, it can at times be difficult to tell how to use it. The links above have some good findings. I've found "response_groups" particularly difficult to figure out. To be safe, you can just include them all. All info below is based on analysis of using these response_groups on a library of 700+ titles.

### GET /1.0/library , GET /1.0/library/{asin}

Included in all groups: asin, purchase_date, sku_lite, status

Additional fields:

* Default/no response_groups provided: authors, available_codecs, category_ladders, content_delivery_type, content_type, format_type, has_children, is_adult_product, is_listenable, issue_date, language, merchandising_summary, narrators, product_images, publisher_name, release_date, runtime_length_min, subtitle, thesaurus_subject_keywords, title, pdf_link, pdf_url, publication_name, content_rating
* badge_types: origin_id, origin_marketplace, origin_type
* category_ladders: category_ladders
* contributors: authors, narrators, publisher_name
* is_downloaded, is_downloaded
* is_returnable: available_codecs, content_delivery_type, content_type, format_type, has_children, is_adult_product, is_listenable, is_returnable, issue_date, language, merchandising_summary, origin_id, origin_marketplace, origin_type, release_date, runtime_length_min, thesaurus_subject_keywords, publication_name, content_rating
* media: product_images
* origin_asin: origin_asin, origin_id, origin_marketplace, origin_type
* pdf_url: pdf_link, pdf_url. (The value of pdf_link and pdf_url seems to always be identical)
* percent_complete: available_codecs, content_delivery_type, content_type, format_type, has_children, is_adult_product, is_listenable, issue_date, language, merchandising_summary, origin_asin, origin_id, origin_marketplace, origin_type, percent_complete, release_date, runtime_length_min, thesaurus_subject_keywords, publication_name, content_rating
* price: price
* product_attrs: available_codecs, content_delivery_type, content_type, format_type, has_children, is_adult_product, is_listenable, issue_date, language, merchandising_summary, release_date, runtime_length_min, thesaurus_subject_keywords, publication_name, content_rating
* product_desc: subtitle, title
* product_extended_attrs: publisher_summary, editorial_reviews, audible_editors_summary
* product_plans: plans
* rating: rating
* relationships: relationships
* reviews: customer_reviews
* sample: sample_url
* series: series
* sku: sku

Strange response_groups:

* provided_review: provided_review. However, provides no results unless used with "rating"
* I can find no combination which gives these response_groups do anything meaningful: claim_code_url, product_plan_details, review_attrs
