using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumen.Api.Graphics;

namespace Lumen.EffectsTest
{
    public class Meteor
    {
        public double _hue;
        public double _pos;
        public bool _bGoingLeft;
        public double _speed;
        public double _meteorSize;
        public bool _bBounce;

        public void Reverse()
        {
            _bGoingLeft = !_bGoingLeft;
        }

        public Meteor(double hue, double pos, bool bGoingLeft, bool bBounce, double speed, double size)
        {
            _hue = hue;
            _pos = pos;
            _bGoingLeft = bGoingLeft;
            _speed = speed;
            _meteorSize = size;
        }

        public void Draw(ILedCanvas graphics)
        {

            _pos = (_bGoingLeft) ? _pos - _speed : _pos + _speed;
            if (_pos < 0)
                _pos += graphics.PixelCount;
            if (_pos >= graphics.PixelCount)
                _pos -= graphics.PixelCount;

            if (_bBounce)
            {
                if (_pos < _meteorSize)
                {
                    _bGoingLeft = false;
                    _pos = _meteorSize;
                }
                if (_pos >= graphics.PixelCount)
                {
                    _bGoingLeft = true;
                    _pos = graphics.PixelCount - 1;
                }
            }

            for (double j = 0; j < _meteorSize; j++)                    // Draw the meteor head
            {
                double x = (_pos - j);
                if (x < graphics.PixelCount && x >= 0)
                {
                    _hue = _hue + 0.50f;
                    _hue %= 360;
                    LedColor color = LedColor.HSVToRGB(_hue, 1.0, 0.8);
                    graphics.DrawPixels(x, 1, color);
                }
            }
        }
    }
}
