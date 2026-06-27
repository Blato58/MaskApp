package cn.com.heaton.shiningmask.ui.utils;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.BitmapShader;
import android.graphics.Camera;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.ColorMatrix;
import android.graphics.ColorMatrixColorFilter;
import android.graphics.LinearGradient;
import android.graphics.Matrix;
import android.graphics.Paint;
import android.graphics.PorterDuff;
import android.graphics.PorterDuffXfermode;
import android.graphics.Rect;
import android.graphics.RectF;
import android.graphics.Shader;
import android.media.ThumbnailUtils;
import android.os.Environment;
import android.renderscript.Allocation;
import android.renderscript.Element;
import android.renderscript.RenderScript;
import android.renderscript.ScriptIntrinsicBlur;
import android.util.Log;
import android.view.View;
import androidx.core.view.ViewCompat;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

/* JADX INFO: loaded from: classes.dex */
public class BitmapUtils {
    private static final String TAG = "BitmapUtils";

    public static Bitmap createReflectedBitmap(Bitmap bitmap, int i) {
        if (bitmap == null) {
            Log.e(TAG, "the srcBitmap is null");
            return null;
        }
        int width = bitmap.getWidth();
        int height = bitmap.getHeight();
        if (width == 0 || height == 0) {
            Log.e(TAG, "the srcBitmap is null");
            return null;
        }
        Matrix matrix = new Matrix();
        matrix.preScale(1.0f, -1.0f);
        try {
            Bitmap bitmapCreateBitmap = Bitmap.createBitmap(bitmap, 0, height - i, width, i, matrix, false);
            if (bitmapCreateBitmap == null) {
                Log.e(TAG, "Create the reflectionBitmap is failed");
                return null;
            }
            Bitmap bitmapCreateBitmap2 = Bitmap.createBitmap(width, height + i, Bitmap.Config.ARGB_8888);
            if (bitmapCreateBitmap2 == null) {
                return null;
            }
            Canvas canvas = new Canvas(bitmapCreateBitmap2);
            canvas.drawBitmap(bitmap, 0.0f, 0.0f, (Paint) null);
            canvas.drawBitmap(bitmapCreateBitmap, 0.0f, height, (Paint) null);
            Paint paint = new Paint();
            paint.setAntiAlias(true);
            float f = height;
            paint.setShader(new LinearGradient(0.0f, f, 0.0f, bitmapCreateBitmap2.getHeight(), 1895825407, ViewCompat.MEASURED_SIZE_MASK, Shader.TileMode.MIRROR));
            paint.setXfermode(new PorterDuffXfermode(PorterDuff.Mode.DST_IN));
            canvas.save();
            canvas.drawRect(0.0f, f, width, bitmapCreateBitmap2.getHeight(), paint);
            if (bitmapCreateBitmap != null && !bitmapCreateBitmap.isRecycled()) {
                bitmapCreateBitmap.recycle();
            }
            canvas.restore();
            return bitmapCreateBitmap2;
        } catch (Exception e) {
            e.printStackTrace();
            Log.e(TAG, "Create the reflectionBitmap is failed");
            return null;
        }
    }

    public static Bitmap getRoundImage(Bitmap bitmap, float f) {
        if (bitmap == null) {
            Log.e(TAG, "the srcBitmap is null");
            return null;
        }
        int width = bitmap.getWidth();
        int height = bitmap.getHeight();
        BitmapShader bitmapShader = new BitmapShader(bitmap, Shader.TileMode.CLAMP, Shader.TileMode.CLAMP);
        Paint paint = new Paint();
        paint.setAntiAlias(true);
        paint.setShader(bitmapShader);
        RectF rectF = new RectF(0.0f, 0.0f, width, height);
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(width, height, Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmapCreateBitmap);
        canvas.drawRoundRect(rectF, f, f, paint);
        canvas.save();
        canvas.restore();
        return bitmapCreateBitmap;
    }

    public static Bitmap skewImage(Bitmap bitmap, float f, int i) {
        if (bitmap == null) {
            Log.e(TAG, "the srcBitmap is null");
            return null;
        }
        Bitmap bitmapCreateReflectedBitmap = createReflectedBitmap(bitmap, i);
        if (bitmapCreateReflectedBitmap == null) {
            Log.e(TAG, "failed to createReflectedBitmap");
            return null;
        }
        int width = bitmapCreateReflectedBitmap.getWidth();
        int height = bitmapCreateReflectedBitmap.getHeight();
        Matrix matrix = new Matrix();
        matrix.postScale(180.0f / width, 270.0f / height);
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(bitmapCreateReflectedBitmap, 0, 0, width, height, matrix, true);
        Camera camera = new Camera();
        camera.save();
        Matrix matrix2 = new Matrix();
        camera.rotateY(f);
        camera.getMatrix(matrix2);
        camera.restore();
        matrix2.preTranslate((-bitmapCreateBitmap.getWidth()) >> 1, (-bitmapCreateBitmap.getHeight()) >> 1);
        Bitmap bitmapCreateBitmap2 = Bitmap.createBitmap(bitmapCreateBitmap, 0, 0, bitmapCreateBitmap.getWidth(), bitmapCreateBitmap.getHeight(), matrix2, true);
        Bitmap bitmapCreateBitmap3 = Bitmap.createBitmap(bitmapCreateBitmap2.getWidth(), bitmapCreateBitmap2.getHeight(), Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmapCreateBitmap3);
        Paint paint = new Paint();
        paint.setAntiAlias(true);
        paint.setFilterBitmap(true);
        canvas.drawBitmap(bitmapCreateBitmap2, 0.0f, 0.0f, paint);
        if (bitmapCreateBitmap != null && !bitmapCreateBitmap.isRecycled()) {
            bitmapCreateBitmap.recycle();
        }
        if (bitmapCreateBitmap2 != null && !bitmapCreateBitmap2.isRecycled()) {
            bitmapCreateBitmap2.recycle();
        }
        canvas.save();
        canvas.restore();
        return bitmapCreateBitmap3;
    }

    public static Bitmap blurBitmap(Bitmap bitmap, float f, Context context) {
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(bitmap.getWidth(), bitmap.getHeight(), Bitmap.Config.ARGB_8888);
        RenderScript renderScriptCreate = RenderScript.create(context);
        ScriptIntrinsicBlur scriptIntrinsicBlurCreate = ScriptIntrinsicBlur.create(renderScriptCreate, Element.U8_4(renderScriptCreate));
        Allocation allocationCreateFromBitmap = Allocation.createFromBitmap(renderScriptCreate, bitmap);
        Allocation allocationCreateFromBitmap2 = Allocation.createFromBitmap(renderScriptCreate, bitmapCreateBitmap);
        if (f > 25.0f) {
            f = 25.0f;
        } else if (f <= 0.0f) {
            f = 1.0f;
        }
        scriptIntrinsicBlurCreate.setRadius(f);
        scriptIntrinsicBlurCreate.setInput(allocationCreateFromBitmap);
        scriptIntrinsicBlurCreate.forEach(allocationCreateFromBitmap2);
        allocationCreateFromBitmap2.copyTo(bitmapCreateBitmap);
        bitmap.recycle();
        renderScriptCreate.destroy();
        return bitmapCreateBitmap;
    }

    public static Bitmap addFrameBitmap(Bitmap bitmap, int i, int i2) {
        if (bitmap == null) {
            Log.e(TAG, "the srcBitmap or borderBitmap is null");
            return null;
        }
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(bitmap.getWidth() + i, bitmap.getHeight() + i, Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmapCreateBitmap);
        Rect clipBounds = canvas.getClipBounds();
        clipBounds.bottom--;
        clipBounds.right--;
        Paint paint = new Paint();
        paint.setColor(i2);
        paint.setStyle(Paint.Style.STROKE);
        paint.setStrokeWidth(i);
        canvas.drawRect(clipBounds, paint);
        float f = i / 2;
        canvas.drawBitmap(bitmap, f, f, (Paint) null);
        canvas.save();
        canvas.restore();
        if (bitmap != null && !bitmap.isRecycled()) {
            bitmap.recycle();
        }
        return bitmapCreateBitmap;
    }

    public static Bitmap lomoFilter(Bitmap bitmap) {
        int width = bitmap.getWidth();
        int height = bitmap.getHeight();
        int[] iArr = new int[width * height];
        bitmap.getPixels(iArr, 0, width, 0, 0, width, height);
        int i = width > height ? (32768 * height) / width : (32768 * width) / height;
        int i2 = width >> 1;
        int i3 = height >> 1;
        int i4 = (i2 * i2) + (i3 * i3);
        int i5 = (int) (i4 * 0.19999999f);
        int i6 = i4 - i5;
        for (int i7 = 0; i7 < height; i7++) {
            int i8 = 0;
            while (i8 < width) {
                int i9 = (i7 * width) + i8;
                int i10 = iArr[i9];
                int iRed = Color.red(i10);
                int iGreen = Color.green(i10);
                int iBlue = Color.blue(i10);
                int i11 = iRed < 128 ? iRed : 256 - iRed;
                int i12 = (((i11 * i11) * i11) / 64) / 256;
                int i13 = i;
                if (iRed >= 128) {
                    i12 = 255 - i12;
                }
                int i14 = iGreen < 128 ? iGreen : 256 - iGreen;
                int i15 = (i14 * i14) / 128;
                if (iGreen >= 128) {
                    i15 = 255 - i15;
                }
                int i16 = (iBlue / 2) + 37;
                int i17 = i2 - i8;
                int i18 = i3 - i7;
                if (width > height) {
                    i17 = (i17 * i13) >> 15;
                } else {
                    i18 = (i18 * i13) >> 15;
                }
                int i19 = (i17 * i17) + (i18 * i18);
                if (i19 > i5) {
                    int i20 = ((i4 - i19) << 8) / i6;
                    int i21 = i20 * i20;
                    i12 = (i12 * i21) >> 16;
                    i15 = (i15 * i21) >> 16;
                    int i22 = (i16 * i21) >> 16;
                    i16 = 255;
                    if (i12 > 255) {
                        i12 = 255;
                    } else if (i12 < 0) {
                        i12 = 0;
                    }
                    if (i15 > 255) {
                        i15 = 255;
                    } else if (i15 < 0) {
                        i15 = 0;
                    }
                    if (i22 <= 255) {
                        i16 = i22 < 0 ? 0 : i22;
                    }
                }
                iArr[i9] = Color.rgb(i12, i15, i16);
                i8++;
                i = i13;
            }
        }
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(width, height, Bitmap.Config.RGB_565);
        bitmapCreateBitmap.setPixels(iArr, 0, width, 0, 0, width, height);
        return bitmapCreateBitmap;
    }

    public static Bitmap oldTimeFilter(Bitmap bitmap) {
        int width = bitmap.getWidth();
        int height = bitmap.getHeight();
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(width, height, Bitmap.Config.RGB_565);
        int[] iArr = new int[width * height];
        bitmap.getPixels(iArr, 0, width, 0, 0, width, height);
        for (int i = 0; i < height; i++) {
            int i2 = 0;
            while (i2 < width) {
                int i3 = (width * i) + i2;
                int i4 = iArr[i3];
                double dRed = Color.red(i4);
                double dGreen = Color.green(i4);
                int[] iArr2 = iArr;
                double dBlue = Color.blue(i4);
                int i5 = (int) ((0.393d * dRed) + (0.769d * dGreen) + (0.189d * dBlue));
                int i6 = (int) ((0.349d * dRed) + (0.686d * dGreen) + (0.168d * dBlue));
                int i7 = (int) ((dRed * 0.272d) + (dGreen * 0.534d) + (dBlue * 0.131d));
                if (i5 > 255) {
                    i5 = 255;
                }
                if (i6 > 255) {
                    i6 = 255;
                }
                if (i7 > 255) {
                    i7 = 255;
                }
                iArr2[i3] = Color.argb(255, i5, i6, i7);
                i2++;
                iArr = iArr2;
            }
        }
        bitmapCreateBitmap.setPixels(iArr, 0, width, 0, 0, width, height);
        return bitmapCreateBitmap;
    }

    public static Bitmap warmthFilter(Bitmap bitmap, int i, int i2) {
        int width = bitmap.getWidth();
        int height = bitmap.getHeight();
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(width, height, Bitmap.Config.RGB_565);
        int iMin = Math.min(i, i2);
        int[] iArr = new int[width * height];
        bitmap.getPixels(iArr, 0, width, 0, 0, width, height);
        int i3 = height - 1;
        for (int i4 = 1; i4 < i3; i4++) {
            int i5 = width - 1;
            int i6 = 1;
            while (i6 < i5) {
                int i7 = (i4 * width) + i6;
                int i8 = iArr[i7];
                int iRed = Color.red(i8);
                int iGreen = Color.green(i8);
                int iBlue = Color.blue(i8);
                int i9 = width;
                int i10 = i3;
                int[] iArr2 = iArr;
                int iPow = (int) (Math.pow(i2 - i4, 2.0d) + Math.pow(i - i6, 2.0d));
                if (iPow < iMin * iMin) {
                    int iSqrt = (int) ((1.0d - (Math.sqrt(iPow) / ((double) iMin))) * 150.0d);
                    iRed += iSqrt;
                    iGreen += iSqrt;
                    iBlue += iSqrt;
                }
                iArr2[i7] = Color.argb(255, Math.min(255, Math.max(0, iRed)), Math.min(255, Math.max(0, iGreen)), Math.min(255, Math.max(0, iBlue)));
                i6++;
                width = i9;
                i3 = i10;
                iArr = iArr2;
            }
        }
        bitmapCreateBitmap.setPixels(iArr, 0, width, 0, 0, width, height);
        return bitmapCreateBitmap;
    }

    public static Bitmap handleImage(Bitmap bitmap, int i, int i2, int i3) {
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(bitmap.getWidth(), bitmap.getHeight(), Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmapCreateBitmap);
        Paint paint = new Paint();
        paint.setAntiAlias(true);
        ColorMatrix colorMatrix = new ColorMatrix();
        ColorMatrix colorMatrix2 = new ColorMatrix();
        ColorMatrix colorMatrix3 = new ColorMatrix();
        ColorMatrix colorMatrix4 = new ColorMatrix();
        float f = (i2 * 1.0f) / 127.0f;
        float f2 = (((i3 - 127) * 1.0f) / 127.0f) * 180.0f;
        colorMatrix3.reset();
        colorMatrix3.setScale(f, f, f, 1.0f);
        colorMatrix2.reset();
        colorMatrix2.setSaturation((i * 1.0f) / 127.0f);
        colorMatrix.reset();
        colorMatrix.setRotate(0, f2);
        colorMatrix.setRotate(1, f2);
        colorMatrix.setRotate(2, f2);
        colorMatrix4.reset();
        colorMatrix4.postConcat(colorMatrix3);
        colorMatrix4.postConcat(colorMatrix2);
        colorMatrix4.postConcat(colorMatrix);
        paint.setColorFilter(new ColorMatrixColorFilter(colorMatrix4));
        canvas.drawBitmap(bitmap, 0.0f, 0.0f, paint);
        return bitmapCreateBitmap;
    }

    public static Bitmap addBigFrame(Bitmap bitmap, Bitmap bitmap2) {
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(bitmap.getWidth(), bitmap.getHeight(), Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmapCreateBitmap);
        Paint paint = new Paint();
        canvas.drawBitmap(bitmap, 0.0f, 0.0f, paint);
        canvas.drawBitmap(Bitmap.createScaledBitmap(bitmap2, bitmap.getWidth(), bitmap.getHeight(), true), 0.0f, 0.0f, paint);
        canvas.save();
        canvas.restore();
        return bitmapCreateBitmap;
    }

    public static Bitmap createBitmap(String str, int i, int i2) {
        double d;
        try {
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.inJustDecodeBounds = true;
            BitmapFactory.decodeFile(str, options);
            int i3 = options.outWidth;
            int i4 = options.outHeight;
            if (i3 < i || i4 < i2) {
                i = i3;
                d = 0.0d;
                i2 = i4;
            } else if (i3 > i4) {
                d = ((double) i3) / ((double) i);
                i2 = (int) (((double) i4) / d);
            } else {
                double d2 = ((double) i4) / ((double) i2);
                i = (int) (((double) i3) / d2);
                d = d2;
            }
            BitmapFactory.Options options2 = new BitmapFactory.Options();
            options2.inSampleSize = ((int) d) + 1;
            options2.inJustDecodeBounds = false;
            options2.outHeight = i2;
            options2.outWidth = i;
            return BitmapFactory.decodeFile(str, options2);
        } catch (Exception unused) {
            return null;
        }
    }

    public static Bitmap createBitmap2(Context context, int i, int i2, int i3) {
        double d;
        try {
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.inJustDecodeBounds = true;
            BitmapFactory.decodeResource(context.getResources(), i, options);
            int i4 = options.outWidth;
            int i5 = options.outHeight;
            if (i4 < i2 || i5 < i3) {
                i2 = i4;
                d = 0.0d;
                i3 = i5;
            } else if (i4 > i5) {
                d = ((double) i4) / ((double) i2);
                i3 = (int) (((double) i5) / d);
            } else {
                double d2 = ((double) i5) / ((double) i3);
                i2 = (int) (((double) i4) / d2);
                d = d2;
            }
            BitmapFactory.Options options2 = new BitmapFactory.Options();
            options2.inSampleSize = ((int) d) + 1;
            options2.inJustDecodeBounds = false;
            options2.outHeight = i3;
            options2.outWidth = i2;
            return BitmapFactory.decodeResource(context.getResources(), i, options2);
        } catch (Exception unused) {
            return null;
        }
    }

    public static String saveToLocalJPG(Bitmap bitmap, String str) throws Throwable {
        FileOutputStream fileOutputStream;
        if (!Environment.getExternalStorageState().equals("mounted")) {
            return null;
        }
        File file = new File(str);
        if (!file.exists()) {
            file.mkdirs();
        }
        String str2 = str + (UUID.randomUUID().toString() + ".jpg");
        File file2 = new File(str2);
        if (file2.exists()) {
            return str2;
        }
        try {
            file2.createNewFile();
            fileOutputStream = new FileOutputStream(str2);
        } catch (IOException unused) {
            fileOutputStream = null;
        } catch (Throwable th) {
            th = th;
            fileOutputStream = null;
        }
        try {
            bitmap.compress(Bitmap.CompressFormat.JPEG, 100, fileOutputStream);
            try {
                fileOutputStream.flush();
                fileOutputStream.close();
                return str2;
            } catch (IOException unused2) {
                return null;
            }
        } catch (IOException unused3) {
            try {
                fileOutputStream.flush();
                fileOutputStream.close();
            } catch (IOException unused4) {
            }
            return null;
        } catch (Throwable th2) {
            th = th2;
            try {
                fileOutputStream.flush();
                fileOutputStream.close();
                throw th;
            } catch (IOException unused5) {
                return null;
            }
        }
    }

    public static String saveToLocalPNG(Bitmap bitmap, int i, int i2) throws Throwable {
        FileOutputStream fileOutputStream;
        FileOutputStream fileOutputStream2;
        File file = new File(Environment.getExternalStorageDirectory(), "diyimage");
        if (!file.exists()) {
            file.mkdir();
        }
        if (!Environment.getExternalStorageState().equals("mounted")) {
            return null;
        }
        String str = file.toString() + "/" + ("anim_" + i2 + "_image_" + i + ".png");
        File file2 = new File(str);
        if (!file2.exists()) {
            try {
                file2.createNewFile();
                fileOutputStream2 = new FileOutputStream(str);
                try {
                    bitmap.compress(Bitmap.CompressFormat.PNG, 100, fileOutputStream2);
                    try {
                        fileOutputStream2.flush();
                        fileOutputStream2.close();
                        return str;
                    } catch (IOException unused) {
                        return null;
                    }
                } catch (IOException unused2) {
                    try {
                        fileOutputStream2.flush();
                        fileOutputStream2.close();
                    } catch (IOException unused3) {
                    }
                    return null;
                } catch (Throwable th) {
                    th = th;
                    try {
                        fileOutputStream2.flush();
                        fileOutputStream2.close();
                        throw th;
                    } catch (IOException unused4) {
                        return null;
                    }
                }
            } catch (IOException unused5) {
                fileOutputStream2 = null;
            } catch (Throwable th2) {
                th = th2;
                fileOutputStream2 = null;
            }
        } else {
            try {
                file2.delete();
                fileOutputStream = new FileOutputStream(str);
                if (bitmap != null) {
                    try {
                        bitmap.compress(Bitmap.CompressFormat.PNG, 100, fileOutputStream);
                    } catch (IOException unused6) {
                        try {
                            fileOutputStream.flush();
                            fileOutputStream.close();
                        } catch (IOException unused7) {
                        }
                        return null;
                    } catch (Throwable th3) {
                        th = th3;
                        try {
                            fileOutputStream.flush();
                            fileOutputStream.close();
                            throw th;
                        } catch (IOException unused8) {
                            return null;
                        }
                    }
                }
                try {
                    fileOutputStream.flush();
                    fileOutputStream.close();
                    return str;
                } catch (IOException unused9) {
                    return null;
                }
            } catch (IOException unused10) {
                fileOutputStream = null;
            } catch (Throwable th4) {
                th = th4;
                fileOutputStream = null;
            }
        }
    }

    public static String saveToLocalPNG(Context context, Bitmap bitmap, String str) throws Throwable {
        FileOutputStream fileOutputStream;
        FileOutputStream fileOutputStream2;
        File externalFilePath = FileUtils.getExternalFilePath(context, str);
        if (!externalFilePath.exists()) {
            externalFilePath.mkdir();
        }
        if (!Environment.getExternalStorageState().equals("mounted")) {
            return null;
        }
        String str2 = externalFilePath.toString() + "/" + System.currentTimeMillis() + ".png";
        File file = new File(str2);
        if (!file.exists()) {
            try {
                file.createNewFile();
                LogUtil.d("保存的图片url:" + str2.toString());
                fileOutputStream = new FileOutputStream(str2);
            } catch (IOException unused) {
                fileOutputStream = null;
            } catch (Throwable th) {
                th = th;
                fileOutputStream = null;
            }
            try {
                bitmap.compress(Bitmap.CompressFormat.PNG, 100, fileOutputStream);
                try {
                    fileOutputStream.flush();
                    fileOutputStream.close();
                    return str2;
                } catch (IOException unused2) {
                    return null;
                }
            } catch (IOException unused3) {
                try {
                    fileOutputStream.flush();
                    fileOutputStream.close();
                } catch (IOException unused4) {
                }
                return null;
            } catch (Throwable th2) {
                th = th2;
                try {
                    fileOutputStream.flush();
                    fileOutputStream.close();
                    throw th;
                } catch (IOException unused5) {
                    return null;
                }
            }
        }
        try {
            file.delete();
            fileOutputStream2 = new FileOutputStream(str2);
            if (bitmap != null) {
                try {
                    bitmap.compress(Bitmap.CompressFormat.PNG, 100, fileOutputStream2);
                } catch (IOException unused6) {
                    try {
                        fileOutputStream2.flush();
                        fileOutputStream2.close();
                    } catch (IOException unused7) {
                    }
                    return null;
                } catch (Throwable th3) {
                    th = th3;
                    try {
                        fileOutputStream2.flush();
                        fileOutputStream2.close();
                        throw th;
                    } catch (IOException unused8) {
                        return null;
                    }
                }
            }
            try {
                fileOutputStream2.flush();
                fileOutputStream2.close();
                return str2;
            } catch (IOException unused9) {
                return null;
            }
        } catch (IOException unused10) {
            fileOutputStream2 = null;
        } catch (Throwable th4) {
            th = th4;
            fileOutputStream2 = null;
        }
    }

    public static Bitmap getImageThumbnail(String str, int i, int i2) {
        BitmapFactory.Options options = new BitmapFactory.Options();
        options.inJustDecodeBounds = true;
        BitmapFactory.decodeFile(str, options);
        options.inJustDecodeBounds = false;
        int i3 = options.outHeight;
        int i4 = options.outWidth / i;
        int i5 = i3 / i2;
        if (i4 >= i5) {
            i4 = i5;
        }
        options.inSampleSize = i4 > 0 ? i4 : 1;
        return ThumbnailUtils.extractThumbnail(BitmapFactory.decodeFile(str, options), i, i2, 2);
    }

    public static Bitmap rotateBitmap(Bitmap bitmap, float f) {
        if (bitmap == null) {
            return null;
        }
        int width = bitmap.getWidth();
        int height = bitmap.getHeight();
        Matrix matrix = new Matrix();
        matrix.setRotate(f);
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(bitmap, 0, 0, width, height, matrix, false);
        if (bitmapCreateBitmap.equals(bitmap)) {
            return bitmapCreateBitmap;
        }
        bitmap.recycle();
        return bitmapCreateBitmap;
    }

    public static Bitmap adjustPhotoRotation(Bitmap bitmap, int i) {
        float height;
        float width;
        Matrix matrix = new Matrix();
        matrix.setRotate(i, bitmap.getWidth() / 2.0f, bitmap.getHeight() / 2.0f);
        if (i == 90) {
            height = bitmap.getHeight();
            width = 0.0f;
        } else {
            height = bitmap.getHeight();
            width = bitmap.getWidth();
        }
        float[] fArr = new float[9];
        matrix.getValues(fArr);
        matrix.postTranslate(height - fArr[2], width - fArr[5]);
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(bitmap.getHeight(), bitmap.getWidth(), Bitmap.Config.ARGB_8888);
        new Canvas(bitmapCreateBitmap).drawBitmap(bitmap, matrix, new Paint());
        return bitmapCreateBitmap;
    }

    public static Bitmap replaceBitmapColor(Bitmap bitmap, int i, int i2) {
        Bitmap bitmapCopy = bitmap.copy(Bitmap.Config.ARGB_8888, true);
        int width = bitmapCopy.getWidth();
        int height = bitmapCopy.getHeight();
        for (int i3 = 0; i3 < height; i3++) {
            for (int i4 = 0; i4 < width; i4++) {
                if (bitmapCopy.getPixel(i4, i3) < -65536) {
                    bitmapCopy.setPixel(i4, i3, i);
                } else {
                    bitmapCopy.setPixel(i4, i3, i2);
                }
            }
        }
        return bitmapCopy;
    }

    public static Bitmap getBitmap(Bitmap bitmap) {
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(bitmap.getWidth(), bitmap.getHeight(), Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmapCreateBitmap);
        canvas.drawColor(0);
        canvas.drawBitmap(bitmap, 0.0f, 0.0f, new Paint());
        return bitmapCreateBitmap;
    }

    /* JADX WARN: Removed duplicated region for block: B:154:0x0169  */
    /* JADX WARN: Removed duplicated region for block: B:156:0x016c  */
    /* JADX WARN: Removed duplicated region for block: B:167:0x0185  */
    /* JADX WARN: Removed duplicated region for block: B:214:0x0208  */
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct code enable 'Show inconsistent code' option in preferences
    */
    public static android.graphics.Bitmap bitmapCrop(android.graphics.Bitmap r16) {
        /*
            Method dump skipped, instruction units count: 691
            To view this dump change 'Code comments level' option to 'DEBUG'
        */
        throw new UnsupportedOperationException("Method not decompiled: cn.com.heaton.shiningmask.ui.utils.BitmapUtils.bitmapCrop(android.graphics.Bitmap):android.graphics.Bitmap");
    }

    public static List<Integer> getBitmapData(Bitmap bitmap) {
        Bitmap bitmapCopy = bitmap.copy(Bitmap.Config.ARGB_8888, true);
        int width = bitmapCopy.getWidth();
        int height = bitmapCopy.getHeight();
        LogUtil.d("图片的宽：" + width + " 高：" + height);
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < width; i++) {
            for (int i2 = 0; i2 < height; i2++) {
                LogUtil.d("X:" + i + "Y:" + i2);
                arrayList.add(Integer.valueOf(bitmapCopy.getPixel(i, i2)));
            }
        }
        return arrayList;
    }

    public static Bitmap getViewBp(View view) {
        if (view == null) {
            return null;
        }
        view.setDrawingCacheEnabled(true);
        view.buildDrawingCache();
        view.measure(View.MeasureSpec.makeMeasureSpec(view.getWidth(), 1073741824), View.MeasureSpec.makeMeasureSpec(view.getHeight(), 1073741824));
        view.layout((int) view.getX(), (int) view.getY(), ((int) view.getX()) + view.getMeasuredWidth(), ((int) view.getY()) + view.getMeasuredHeight());
        Bitmap bitmapCreateBitmap = Bitmap.createBitmap(view.getDrawingCache(), 0, 0, view.getMeasuredWidth(), view.getMeasuredHeight());
        view.setDrawingCacheEnabled(false);
        view.destroyDrawingCache();
        return bitmapCreateBitmap;
    }

    public static Bitmap captureView(View view) {
        try {
            Bitmap bitmapCreateBitmap = Bitmap.createBitmap(view.getWidth(), view.getHeight(), Bitmap.Config.ARGB_8888);
            view.draw(new Canvas(bitmapCreateBitmap));
            return bitmapCreateBitmap;
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }
}