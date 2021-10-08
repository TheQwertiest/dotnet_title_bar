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
using Qwr.ComponentInterface;
using System;
using System.Drawing;
using System.Xml.Linq;

namespace fooTitle.Layers
{
    [LayerType("album-art")]
    public class AlbumArtLayer : Layer
    {
        protected Bitmap albumArt;
        protected Bitmap albumArtStub;
        protected Bitmap noCover;
        /// <summary>
        /// This is a scaled copy of albumArt. This prevents rescaling
        /// a large bitmap every frame.
        /// </summary>
        protected Bitmap cachedResized;
        private int timesCheckedArtwork = 0;

        public AlbumArtLayer(Rectangle parentRect, XElement node, Skin skin)
            : base(parentRect, node, skin)
        {
            try
            {
                XElement contents = GetFirstChildByName(node, "contents");
                XElement NoAlbumArt = GetFirstChildByName(contents, "NoAlbumArt");
                string name = GetNodeValue(NoAlbumArt);
                noCover = ParentSkin.GetSkinImage(name);
            }
            catch (Exception)
            {
                noCover = null;
            }

            ParentSkin.PlaybackAdvancedToNewTrack += CurrentSkin_OnPlaybackNewTrackEvent;
            ParentSkin.TrackPlaybackPositionChanged += CurrentSkin_OnPlaybackTimeEvent;
            ParentSkin.PlaybackStopped += CurrentSkin_OnPlaybackStopEvent;
        }

        private void LoadArtwork(IMetadbHandle song)
        {
            Console.Get().LogInfo("Loading album art... ");
            Bitmap artwork = song.Artwork(ArtId.CoverFront);
            if (artwork != null)
            {
                try
                {
                    albumArt = new Bitmap(artwork);
                    artwork.Dispose();
                }
                catch (Exception e)
                {
                    albumArt = null;
                    Console.Get().LogWarning($"Cannot open album art {song.Path()} : {e.Message}");
                }
            }
            Bitmap artworkStub = song.ArtworkStub(ArtId.CoverFront);
            if (artworkStub != null)
            {
                try
                {
                    albumArtStub = new Bitmap(artworkStub);
                    artworkStub.Dispose();
                }
                catch (Exception e)
                {
                    albumArtStub = null;
                    Console.Get().LogWarning($"Cannot open album art stub {song.Path()} : {e.Message}");
                }
            }
        }

        private void CurrentSkin_OnPlaybackStopEvent(PlaybackStopReason reason)
        {
            if (reason != PlaybackStopReason.StartingAnother)
            {
                timesCheckedArtwork = 0;
                albumArt = null;
                albumArtStub = null;
                cachedResized = null;
            }
        }

        private void CurrentSkin_OnPlaybackTimeEvent(double time)
        {
            if (albumArt == null && time % Configs.Display_ArtLoadRetryFrequency.Value == 0
                && (timesCheckedArtwork < Configs.Display_ArtLoadMaxRetries.Value || Configs.Display_ArtLoadMaxRetries.Value == -1))
            {
                timesCheckedArtwork++;
                LoadArtwork(Main.Get().Fb2kPlaybackControls.NowPlaying());
            }
        }

        private void CurrentSkin_OnPlaybackNewTrackEvent(IMetadbHandle song)
        {
            timesCheckedArtwork = 0;
            albumArt = null;
            albumArtStub = null;
            cachedResized = null;
            LoadArtwork(song);
        }

        protected override void DrawImpl(Graphics canvas)
        {
            Bitmap toDraw = prepareCachedImage();
            if (toDraw != null)
            {
                canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                canvas.DrawImage(toDraw, ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);
            }
        }

        private Bitmap prepareCachedImage()
        {
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

            using (Graphics canvas = Graphics.FromImage(cachedResized))
            {
                canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                if (noCover != null)
                {
                    canvas.DrawImage(noCover, 0, 0, ClientRect.Width, ClientRect.Height);
                }
                canvas.DrawImage(artOrStub, (ClientRect.Width - scaledWidth) / 2, (ClientRect.Height - scaledHeight) / 2, scaledWidth, scaledHeight);
            }

            return cachedResized;
        }
    }
}
