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
        private readonly ArtId _artId;
        private readonly Bitmap _noCover;
        private Bitmap _albumArt;
        private Bitmap _albumArtStub;
        /// <summary>
        /// This is a scaled copy of albumArt. This prevents rescaling
        /// a large bitmap every frame.
        /// </summary>
        private Bitmap _cachedResized;
        private int _timesCheckedArtwork = 0;

        public AlbumArtLayer(Rectangle parentRect, XElement node, Skin skin)
            : base(parentRect, node, skin)
        {
            var contents = GetFirstChildByName(node, "contents");
            var noAlbumArt = GetFirstChildByNameOrNull(contents, "NoAlbumArt");
            if (noAlbumArt != null)
            {
                string name = GetNodeValue(noAlbumArt);
                _noCover = ParentSkin.GetSkinImage(name);
            }

            var artIdStr = GetCastedAttributeValue(contents, "art-id", "cover-front");
            _artId = artIdStr switch
            {
                "cover-front" => ArtId.CoverFront,
                "cover-back" => ArtId.CoverBack,
                "disc" => ArtId.Disc,
                "icon" => ArtId.Icon,
                "artist" => ArtId.Artist,
                _ => throw new Exception($"Unknown art id: `{artIdStr}`"),
            };

            ParentSkin.PlaybackAdvancedToNewTrack += CurrentSkin_OnPlaybackNewTrackEvent;
            ParentSkin.TrackPlaybackPositionChanged += CurrentSkin_OnPlaybackTimeEvent;
            ParentSkin.PlaybackStopped += CurrentSkin_OnPlaybackStopEvent;
        }

        private void LoadArtwork(IMetadbHandle song)
        {
            Console.Get().LogInfo("Loading album art... ");

            using var artwork = song.Artwork(_artId);
            if (artwork != null)
            {
                try
                {
                    _albumArt = new Bitmap(artwork);
                }
                catch (Exception e)
                {
                    _albumArt = null;
                    Console.Get().LogWarning($"Cannot open album art {song.Path()}:", e);
                }
            }

            using var artworkStub = song.ArtworkStub(_artId);
            if (artworkStub != null)
            {
                try
                {
                    _albumArtStub = new Bitmap(artworkStub);
                }
                catch (Exception e)
                {
                    _albumArtStub = null;
                    Console.Get().LogWarning($"Cannot open album art stub {song.Path()}:", e);
                }
            }
        }

        private void CurrentSkin_OnPlaybackStopEvent(PlaybackStopReason reason)
        {
            if (reason != PlaybackStopReason.StartingAnother)
            {
                _timesCheckedArtwork = 0;
                _albumArt = null;
                _albumArtStub = null;
                _cachedResized = null;
            }
        }

        private void CurrentSkin_OnPlaybackTimeEvent(double time)
        {
            if (_albumArt == null && time % Configs.Display_ArtLoadRetryFrequency.Value == 0
                && (_timesCheckedArtwork < Configs.Display_ArtLoadMaxRetries.Value || Configs.Display_ArtLoadMaxRetries.Value == -1))
            {
                _timesCheckedArtwork++;
                LoadArtwork(Main.Get().Fb2kPlaybackControls.NowPlaying());
            }
        }

        private void CurrentSkin_OnPlaybackNewTrackEvent(IMetadbHandle song)
        {
            _timesCheckedArtwork = 0;
            _albumArt = null;
            _albumArtStub = null;
            _cachedResized = null;
            LoadArtwork(song);
        }

        protected override void DrawImpl(Graphics canvas)
        {
            Bitmap toDraw = PrepareCachedImage();
            if (toDraw != null)
            {
                canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                canvas.DrawImage(toDraw, ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);
            }
        }

        private Bitmap PrepareCachedImage()
        {
            if (ClientRect.Width <= 0 || ClientRect.Height <= 0)
            {
                return null;
            }

            if (_cachedResized != null && _cachedResized.Width == ClientRect.Width && _cachedResized.Height == ClientRect.Height)
            {
                return _cachedResized;
            }

            if (_albumArt == null && _albumArtStub == null)
            {
                return _noCover;
            }

            Bitmap artOrStub = _albumArt ?? _albumArtStub;
            _cachedResized = new Bitmap(ClientRect.Width, ClientRect.Height);

            float scale = Math.Min((float)ClientRect.Width / artOrStub.Width, (float)ClientRect.Height / artOrStub.Height);
            float scaledWidth = artOrStub.Width * scale;
            float scaledHeight = artOrStub.Height * scale;

            using (Graphics canvas = Graphics.FromImage(_cachedResized))
            {
                canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                if (_noCover != null)
                {
                    canvas.DrawImage(_noCover, 0, 0, ClientRect.Width, ClientRect.Height);
                }
                canvas.DrawImage(artOrStub, (ClientRect.Width - scaledWidth) / 2, (ClientRect.Height - scaledHeight) / 2, scaledWidth, scaledHeight);
            }

            return _cachedResized;
        }
    }
}
