{% assign local_showcase = include.showcase %}
### {{ local_showcase.name }}
{: .no_toc}

Author: {{ local_showcase.author }}  
{% if local_showcase.fixed_link -%}
Link to fixed version: {{ local_showcase.fixed_link }}<br/>
{% if local_showcase.original_link %}Original link: {{ local_showcase.original_link }}<br/>{% endif -%}
{%- else -%}
{% if local_showcase.original_link %}Link: {{ local_showcase.original_link }}<br/>{% endif -%}
{%- endif %}