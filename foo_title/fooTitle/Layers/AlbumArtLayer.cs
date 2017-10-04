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
using System.Drawing;
using System.Xml.Linq;
using fooManagedWrapper;

namespace fooTitle.Layers {
    [LayerTypeAttribute("album-art")]
    internal class AlbumArtLayer : Layer {
        protected Bitmap albumArt;
        protected Bitmap albumArtStub;
        protected Bitmap noCover;
        /// <summary>
        /// This is a scaled copy of albumArt. This prevents rescaling
        /// a large bitmap every frame.
        /// </summary>
        protected Bitmap cachedResized;
        private int timesCheckedArtwork = 0;

        public AlbumArtLayer(Rectangle parentRect, XElement node) : base(parentRect, node) {
            try {
                XElement contents = GetFirstChildByName(node, "contents");
                XElement NoAlbumArt = GetFirstChildByName(contents, "NoAlbumArt");
                string name = GetNodeValue(NoAlbumArt);
                noCover = Main.GetInstance().CurrentSkin.GetSkinImage(name);
            } catch (Exception) {
                noCover = null;
            }

            Main.GetInstance().CurrentSkin.OnPlaybackNewTrackEvent += CurrentSkin_OnPlaybackNewTrackEvent;
            Main.GetInstance().CurrentSkin.OnPlaybackTimeEvent += CurrentSkin_OnPlaybackTimeEvent;
            Main.GetInstance().CurrentSkin.OnPlaybackStopEvent += CurrentSkin_OnPlaybackStopEvent;
        }

        private void LoadArtwork(CMetaDBHandle song) {
            CConsole.Write("Loading album art... ");
            Bitmap artwork = song.GetArtworkBitmap(false);
            if (artwork != null) {
                try {
                    albumArt = new Bitmap(artwork);
                    artwork.Dispose();
                } catch (Exception e) {
                    albumArt = null;
                    CConsole.Warning($"Cannot open album art {song.GetPath()} : {e.Message}");
                }
            }
            Bitmap artworkStub = song.GetArtworkBitmap(true);
            if (artworkStub != null) {
                try {
                    albumArtStub = new Bitmap(artworkStub);
                    artworkStub.Dispose();
                } catch (Exception e) {
                    albumArtStub = null;
                    CConsole.Warning($"Cannot open album art stub {song.GetPath()} : {e.Message}");
                }
            }
        }

        private void CurrentSkin_OnPlaybackStopEvent(IPlayControl.StopReason reason) {
            if (reason != IPlayControl.StopReason.stop_reason_starting_another) {
                timesCheckedArtwork = 0;
                albumArt = null;
                albumArtStub = null;
                cachedResized = null;
            }
        }

        private void CurrentSkin_OnPlaybackTimeEvent(double time) {
            if (albumArt == null && time % Main.GetInstance().ArtReloadFreq == 0 
                && (timesCheckedArtwork < Main.GetInstance().ArtReloadMax || Main.GetInstance().ArtReloadMax == -1)) {
                timesCheckedArtwork++;
                LoadArtwork(Main.PlayControl.GetNowPlaying());
            }
        }

        private void CurrentSkin_OnPlaybackNewTrackEvent(CMetaDBHandle song) {
            timesCheckedArtwork = 0;
            albumArt = null;
            albumArtStub = null;
            cachedResized = null;
            LoadArtwork(song);
        }

        protected override void DrawImpl() {
            Bitmap toDraw = prepareCachedImage();
            if (toDraw != null) {
                Display.Canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                Display.Canvas.DrawImage(toDraw, ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);
            }
        }

        private Bitmap prepareCachedImage() {
            if (ClientRect.Width <= 0 || ClientRect.Height <= 0)
                return null;

            if (cachedResized != null && cachedResized.Width == ClientRect.Width && cachedResized.Height == ClientRect.Height)
                return cachedResized;

            if (albumArt == null && albumArtStub == null)
                return noCover;

            Bitmap artOrStub = albumArt ?? albumArtStub;
            cachedResized = new Bitmap(ClientRect.Width, ClientRect.Height);

            float scale = Math.Min((float)ClientRect.Width / artOrStub.Width, (float)ClientRect.Height / artOrStub.Height);
            float scaledWidth = artOrStub.Width * scale;
            float scaledHeight = artOrStub.Height * scale;

            using (Graphics canvas = Graphics.FromImage(cachedResized)) {
                canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                if (noCover != null) {
                    canvas.DrawImage(noCover, 0, 0, ClientRect.Width, ClientRect.Height);
                }
                canvas.DrawImage(artOrStub, (ClientRect.Width - scaledWidth) / 2, (ClientRect.Height - scaledHeight) / 2, scaledWidth, scaledHeight);
            }

            return cachedResized;
        }
    }
}
