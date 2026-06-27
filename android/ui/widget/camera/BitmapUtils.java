package cn.com.heaton.shiningmask.ui.widget.camera;

import android.graphics.Bitmap;
import android.graphics.Matrix;
import android.hardware.Camera;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;

/* JADX INFO: loaded from: classes.dex */
public class BitmapUtils {
    public static Bitmap setTakePicktrueOrientation(int i, Bitmap bitmap) {
        if (bitmap.getWidth() < bitmap.getHeight()) {
            return bitmap;
        }
        Camera.CameraInfo cameraInfo = new Camera.CameraInfo();
        Camera.getCameraInfo(i, cameraInfo);
        return rotaingImageView(i, cameraInfo.orientation, bitmap);
    }

    private static Bitmap rotaingImageView(int i, int i2, Bitmap bitmap) {
        Matrix matrix = new Matrix();
        matrix.postRotate(i2);
        if (i == 1) {
            matrix.postScale(-1.0f, 1.0f);
        }
        return Bitmap.createBitmap(bitmap, 0, 0, bitmap.getWidth(), bitmap.getHeight(), matrix, true);
    }

    public static boolean saveBitmap(Bitmap bitmap, String str) {
        try {
            File file = new File(str);
            File parentFile = file.getParentFile();
            if (!parentFile.exists()) {
                parentFile.mkdirs();
            }
            FileOutputStream fileOutputStream = new FileOutputStream(file);
            boolean zCompress = bitmap.compress(Bitmap.CompressFormat.JPEG, 100, fileOutputStream);
            fileOutputStream.flush();
            fileOutputStream.close();
            return zCompress;
        } catch (FileNotFoundException e) {
            e.printStackTrace();
            return false;
        } catch (IOException e2) {
            e2.printStackTrace();
            return false;
        }
    }
}