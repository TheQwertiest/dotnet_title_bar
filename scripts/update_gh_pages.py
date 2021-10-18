#!/usr/bin/env python3

import argparse
import shutil
from pathlib import Path
from typing import Union

import call_wrapper

PathLike = Union[str, Path]

def update(gh_pages_dir: PathLike):
    cur_dir = Path(__file__).parent.absolute()
    root_dir = cur_dir.parent

    gh_pages_dir = Path(gh_pages_dir).resolve()
    assert(gh_pages_dir.exists() and gh_pages_dir.is_dir())

    shutil.copy2(root_dir/"CHANGELOG.md", gh_pages_dir/"_includes"/"CHANGELOG.md")

if __name__ == '__main__':
    cur_dir = Path(__file__).parent.absolute()
    root_dir = cur_dir.parent
    gh_pages_dir = root_dir/"gh-pages"

    parser = argparse.ArgumentParser(description='Update GitHub Pages content')
    parser.add_argument('--gh_pages_dir', default=gh_pages_dir)

    args = parser.parse_args()

    call_wrapper.final_call_decorator(
        "Updating GitHub Pages content",
        "Updating GitHub Pages content: success",
        "Updating GitHub Pages content: failure!"
    )(
        update
    )(
        gh_pages_dir
    )

