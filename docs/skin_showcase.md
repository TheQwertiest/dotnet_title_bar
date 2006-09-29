---
title: Skin showcase
nav_order: 5
---

# Skin showcase
{: .no_toc }

## Table of contents
{: .no_toc .text-delta }

* TOC
{:toc}

---

## Quick navigate

{% assign collections = "" | split: ',' %}
{% assign collection_types = "" | split: ',' %}
{% assign collections = collections | push: site.showcase_sample %}
{% assign collection_types = collection_types | push: "Built-in" %}
{% assign collections = collections | push: site.showcase_user %}
{% assign collection_types = collection_types | push: "User" %}
{% include functions/showcase_table.html collections=collections collection_types=collection_types %}

## Built-in scripts

These scripts are automatically installed with the component.  

They can be selected in `Preferences`>`Display`>`Title Bar`>`Main`>`Installed Skins`.

{% include functions/showcase_grid.md collection=site.showcase_sample screenshots_dir='/assets/img/screenshots/skins' description_func='showcase_skin_desc' %}

## User scripts

These scripts are created by `Title Bar` users and are not installed with the component.  

Drag-n-drop archives to the skin list inside `Preferences`>`Display`>`Title Bar`>`Main`>`Installed Skins` to install them.

{% include functions/showcase_grid.md collection=site.showcase_user screenshots_dir='/assets/img/screenshots/skins' description_func='showcase_skin_desc' %}

{% include functions/showcase_grid_script.md %}
