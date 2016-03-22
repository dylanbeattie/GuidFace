using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuidFace {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private int i = 0;
        private Guid faceGuid;
        private Guid moodGuid;
        private void Form1_KeyPress(object sender, KeyPressEventArgs e) {
            if (++i > 3) i = 0;
            if (i == 0) {
                faceGuid = Guid.NewGuid();
            }
            moodGuid = Guid.NewGuid();
            DrawFace(faceGuid, moodGuid);
        }

        public void DrawFace(Guid faceGuid, Guid moodGuid) {
            textBox1.Text = faceGuid.ToString();
            var bytes = faceGuid.ToByteArray();
            var r = Clip(0x99 + +(0.2 * bytes[1]) + (0.2 * bytes[0]));
            var g = Clip(r / 1.6 + (0.1 * bytes[2]) + (0.2 * bytes[0]));
            var b = Clip(r / 2.2 + (0.1 * bytes[3]) + (0.2 * bytes[0]));
            var skin = Color.FromArgb(r, g, b);// , Clip(0xCC - bytes[1]), Clip(0x99 - bytes[2]));
            var darkSkin = Color.FromArgb(Clip(r - 20), Clip(g - 20), Clip(b - 20));
            using (var gr = pictureBox1.CreateGraphics()) {
                gr.FillRectangle(new SolidBrush(Color.White), 0, 0, pictureBox1.Width, pictureBox1.Height);
                var s = 200;
                var cx = pictureBox1.Width / 2;
                var cy = pictureBox1.Height / 2;
                var d = 0.8f + (0.4f * bytes[5] / 255f);
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.FillEllipse(new SolidBrush(skin), cx - s, cy - s, 2 * s, 2 * s);

                gr.Clip = new Region(new Rectangle(0, cy, pictureBox1.Width, pictureBox1.Height));

                var chinCentre = new Point(cx, cy + 20 + bytes[9] / 2);
                var chinWidth = 100 + (int)((1.6 * s) - (bytes[10] >> 1));
                var chinHeight = 100 + (int)((1.6 * s) - (bytes[11] >> 1));
                var chinRect = new Rectangle(chinCentre.X - chinWidth / 2, chinCentre.Y - chinHeight / 2, chinWidth, chinHeight);
                gr.FillEllipse(new SolidBrush(skin), chinRect);
                gr.ResetClip();
                gr.InterpolationMode = InterpolationMode.Bicubic;
                var eyes = Color.FromArgb(0x99 + bytes[9] >> 1, 0x99 + bytes[10] >> 1, 0x99 + bytes[11] >> 1);
                var eyeX = 70;
                var eyeY = 30;

                var wow = (moodGuid.ToByteArray()[4] - 128) / 8;
                gr.FillRadialEllipse(new SolidBrush(Color.White), cx - eyeX, cy + eyeY, 50, 25 + wow);
                gr.FillRadialEllipse(new SolidBrush(Color.White), cx + eyeX, cy + eyeY, 50, 25 + wow);

                var dilation = moodGuid.ToByteArray()[1] / 20;
                var lr = (moodGuid.ToByteArray()[2] - 128) / 4;
                var ud = (moodGuid.ToByteArray()[3] - 128) / 7;
                gr.FillRadialEllipse(new SolidBrush(eyes), cx + lr - eyeX, cy + +ud + eyeY, 30, 30);
                gr.FillRadialEllipse(new SolidBrush(eyes), cx + lr + eyeX, cy + ud + eyeY, 30, 30);
                gr.FillRadialEllipse(new SolidBrush(Color.Black), cx + lr - eyeX, cy + +ud + eyeY, 10 + dilation, 10 + dilation);
                gr.FillRadialEllipse(new SolidBrush(Color.Black), cx + lr + eyeX, cy + ud + eyeY, 10 + dilation, 10 + dilation);
                using (var pen = new Pen(skin, 50.0f)) {
                    gr.Clip = new Region(new Rectangle(0, 0, cx, pictureBox1.Height));
                    gr.DrawRadialEllipse(pen, cx - eyeX, cy + eyeY, 75, 50 + wow);
                    gr.Clip = new Region(new Rectangle(cx, 0, pictureBox1.Width, pictureBox1.Height));
                    gr.DrawRadialEllipse(pen, cx + eyeX, cy + eyeY, 75, 50 + wow);
                    gr.ResetClip();
                }
                using (var pen = new Pen(darkSkin, 3.0f)) {
                    gr.Clip = new Region(new Rectangle(0, 0, cx, pictureBox1.Height));
                    gr.DrawRadialEllipse(pen, cx - eyeX, cy + eyeY, 50, 25 + wow);
                    gr.Clip = new Region(new Rectangle(cx, 0, pictureBox1.Width, pictureBox1.Height));
                    gr.DrawRadialEllipse(pen, cx + eyeX, cy + eyeY, 50, 25 + wow);
                    gr.ResetClip();
                }
                var noseWidth = 10 + (bytes[6] / 10);
                var noseHeight = 5 + bytes[7] / 4;
                var noseFlatness = 10 + (bytes[9] / 12);
                gr.DrawCurve(new Pen(darkSkin, 10.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round },
                    new[] {
                        new Point(cx - noseWidth, cy+100),
                        new Point(cx-noseFlatness, cy + 100 + noseHeight),
                        new Point(cx+noseFlatness, cy + 100 + noseHeight),
                        new Point(cx + noseWidth, cy+100),
                    }
                );
                gr.DrawCurve(new Pen(Color.Black, 10.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round },
                    new[] {
                        new Point(cx - 120, cy - Math.Abs(wow)),
                        new Point(cx - 80 + (lr/2), (cy - 20) + ud),
                        new Point(cx - 20, cy - Math.Abs(wow))
                    }
                );


                gr.DrawCurve(new Pen(Color.Black, 10.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round },
                    new[] {
                        new Point(cx + 120, cy - Math.Abs(wow)),
                        new Point(cx + (80 - (lr/2)), (cy - 20) + ud),
                        new Point(cx + 20, cy - Math.Abs(wow))
                    }
                );
                var foo = (128 - moodGuid.ToByteArray()[9]) / 9;
                var bar = (128 - moodGuid.ToByteArray()[10]) / 9;
                var baz = 30 + moodGuid.ToByteArray()[10] / 7;

                gr.DrawCurve(
                    new Pen(darkSkin, 20.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round },
                    new[] {
                        new Point(cx - baz, cy + 170 + foo),
                        new Point(cx, cy + 170 + bar),
                        new Point(cx + baz, cy + 170 + foo),
                    }

                    );


                gr.DrawCurve(
                    new Pen(Color.Black, 2.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round },
                    new[] {
                        new Point(cx - baz, cy + 170 + foo),
                        new Point(cx, cy + 170 + bar),
                        new Point(cx + baz, cy + 170 + foo),
                    }

                    );
                var hairShape = new GraphicsPath(FillMode.Alternate);
                var face = faceGuid.ToByteArray();
                var hair = cy - 200;
                //hairShape.AddClosedCurve(new Point[] {
                //    new Point(cx -200,hair + face[9] & 0x40/2),
                //    new Point(cx - 160, hair + (face[10] & 0x40)/2),
                //    new Point(cx - 120, hair + (face[11] & 0x40)/2),
                //    new Point(cx - 80, hair + face[12] & 0x40/2),
                //    new Point(cx - 40, hair + face[13] & 0x40/2 ),
                //    new Point(cx, hair + (face[10] >> 2)/2 ),
                //    new Point(cx + 40, hair + (face[11] >> 2)/2 ),
                //    new Point(cx + 80, hair + (face[12] >> 2)/2 ),
                //    new Point(cx + 120, hair + (face[13] >> 2)/2 ),
                //    new Point(cx + 160, hair + (face[9] & 0x40)/2 ),
                //    new Point(cx + 200, hair + face[11] & 0x40 ),
                //    new Point(cx+100, cy - 100),
                //    new Point(cx-100, cy - 100),
                //    new Point(cx - 200, cy) 
                //});
                //gr.FillPath(new SolidBrush(Color.Black), hairShape);
            }
        }


        private int Clip(double b) {
            return (int)(b < 0 ? 0 : (b > 0xFF ? (0xFF) : (b)));
        }

        private void Form1_Load(object sender, EventArgs e) {

        }
    }

    public static class GraphicsExtensions {
        public static void FillRadialEllipse(this Graphics g, Brush brush, int x, int y, int xRadius, int yRadius) {
            var rect = new Rectangle(x - xRadius, y - yRadius, xRadius * 2, yRadius * 2);
            g.FillEllipse(brush, rect);
        }

        public static void DrawRadialEllipse(this Graphics g, Pen pen, int x, int y, int xRadius, int yRadius) {
            var rect = new Rectangle(x - xRadius, y - yRadius, xRadius * 2, yRadius * 2);
            g.DrawEllipse(pen, rect);
        }
    }
}
