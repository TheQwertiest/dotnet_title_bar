#!/usr/bin/env python3

import argparse
import importlib
import importlib.util
import sys
import traceback
from pathlib import Path
from typing import Union

import call_wrapper
import download_submodules

PathLike = Union[str, Path]

def call_decorator(command_name: str):
    def f_decorator(f):
        def wrapper(*args, **kwds):
            print(f">> {command_name}: starting")
            try:
                f(*args, **kwds)
                print(f"<< {command_name}: success")
            except Exception:
                traceback.print_exc(file=sys.stderr)
                print(f"<< {command_name}: failure", file=sys.stderr)
                raise call_wrapper.SkippedError()

        return wrapper

    return f_decorator

def load_module(script_path):
    spec = importlib.util.spec_from_file_location("module.name", script_path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod

def setup( skip_submodules_download,
           skip_submodules_patches ):
    cur_dir = Path(__file__).parent.absolute()
    root_dir = cur_dir.parent
    scripts_path = root_dir/'submodules'/'fb2k_utils'/'scripts'

    if (not skip_submodules_download):
        call_decorator('Downloading submodules')(download_submodules.download)()

    call_decorator('Version props file generation')(
        load_module(scripts_path/'generate_cs_version_prop.py').generate_props_custom
    )(
        repo_dir=root_dir,
        output_dir=root_dir/'dotnet_title_bar'
    )

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Setup project')
    parser.add_argument('--skip_submodules_download', default=False, action='store_true')
    parser.add_argument('--skip_submodules_patches', default=False, action='store_true')

    args = parser.parse_args()

    call_wrapper.final_call_decorator(
        "Preparing project repo",
        "Setup complete!",
        "Setup failed!"
    )(
    setup
    )(
        args.skip_submodules_download,
        args.skip_submodules_patches
    )


