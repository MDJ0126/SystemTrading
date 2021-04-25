using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

public class ScreenCapture
{
    // 사용예: ScreenCopy.Copy("test.png");
    // 
    public static void Copy(string outputFilename)
    {            
        // 주화면의 크기 정보 읽기
        Rectangle rect = Screen.PrimaryScreen.Bounds;            
        // 2nd screen = Screen.AllScreens[1]

        // 픽셀 포맷 정보 얻기 (Optional)
        int bitsPerPixel = Screen.PrimaryScreen.BitsPerPixel;
        PixelFormat pixelFormat = PixelFormat.Format32bppArgb;
        if (bitsPerPixel <= 16)
        {
            pixelFormat = PixelFormat.Format16bppRgb565;
        }
        if (bitsPerPixel == 24)
        {
            pixelFormat = PixelFormat.Format24bppRgb;
        }

        // 화면 크기만큼의 Bitmap 생성
        Bitmap bmp = new Bitmap(rect.Width, rect.Height, pixelFormat);

        // Bitmap 이미지 변경을 위해 Graphics 객체 생성
        using (Graphics gr = Graphics.FromImage(bmp))
        {
            // 화면을 그대로 카피해서 Bitmap 메모리에 저장
            gr.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size);
        }

        // Bitmap 데이타를 파일로 저장
        bmp.Save(outputFilename);
        bmp.Dispose();
    }
}
