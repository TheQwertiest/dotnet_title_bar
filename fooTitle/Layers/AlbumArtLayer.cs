/*
    Copyright 2005 - 2006 Roman Plasil
	http://foo-title.sourceforge.net
    This file is part of foo_title.

    foo_title is free software; you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation; either version 2.1 of the License, or
    (at your option) any later version.

    foo_title is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with foo_title; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Xml;
using fooManagedWrapper;

namespace fooTitle.Layers {
    [LayerTypeAttribute("album-art")]
    class AlbumArtLayer : Layer {

        protected Bitmap albumArt;
        protected Bitmap noCover;

        public AlbumArtLayer(Rectangle parentRect, XmlNode node) : base(parentRect, node) {
            try {
                XmlNode contents = GetFirstChildByName(node, "contents");
                XmlNode NoAlbumArt = GetFirstChildByName(contents, "NoAlbumArt");
                string name = GetNodeValue(NoAlbumArt);
                string noCoverPath = Main.GetInstance().CurrentSkin.GetSkinFilePath(name);
                noCover = new Bitmap(noCoverPath);
            } catch (Exception) {
                noCover = null;
            }

            Main.GetInstance().CurrentSkin.OnPlaybackNewTrackEvent += new OnPlaybackNewTrackDelegate(CurrentSkin_OnPlaybackNewTrackEvent);
        }

        protected string findAlbumArt(CMetaDBHandle song, string filenames) {
            String songPath = song.GetPath();
            // remove this prefix
            if (songPath.StartsWith("file:\\"))
                songPath = songPath.Substring(6);
            else if (songPath.StartsWith("file://"))
                songPath = songPath.Substring(7);

            string[] extensions = new string[] { ".jpg", ".png", ".bmp", ".gif", ".jpeg" };
            string[] splitFiles = filenames.Split(new char[] { ';' });
            string[] formattedFiles = new string[splitFiles.Length];
            int i = 0;
            foreach (string f in splitFiles) {
                formattedFiles[i] = Main.PlayControl.FormatTitle(song, f);
                i++;
            }

            foreach (string f in formattedFiles) {
                foreach (string ext in extensions) {
                    String path = Path.Combine(Path.GetDirectoryName(songPath), f + ext);
                    if (File.Exists(path))
                        return path;
                }
            }

            return null;
        }

        void CurrentSkin_OnPlaybackNewTrackEvent(fooManagedWrapper.CMetaDBHandle song) {
            string fileName = findAlbumArt(song, Main.GetInstance().AlbumArtFilenames.Value);
            if (fileName != null) {
                try {
                    Bitmap tmp = new Bitmap(fileName);
                    albumArt = new Bitmap(tmp);
                    tmp.Dispose();
                } catch (Exception e) {
                    albumArt = null;
                    fooManagedWrapper.CConsole.Warning(String.Format("Cannot open album art {0} : {1}", fileName, e.Message));
                }
            } else {
                albumArt = null;
            }
        }

		public override void Draw() {
            Bitmap toDraw;
            if (albumArt != null) {
                toDraw = albumArt;
            } else {
                toDraw = noCover;
            }

            if (toDraw != null) {
                Display.Canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                Display.Canvas.DrawImage(toDraw, ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);
            }
			base.Draw();
		}

    }
}
