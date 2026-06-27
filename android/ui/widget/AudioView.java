package cn.com.heaton.shiningmask.ui.widget;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.util.AttributeSet;
import android.util.Log;
import android.view.View;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import java.util.Arrays;

/* JADX INFO: loaded from: classes.dex */
public class AudioView extends View {
    private static final int LUMP_COLOR = Color.parseColor("#998AA8");
    private static final int LUMP_COUNT = 24;
    private static final int LUMP_HEIGHT = 20;
    private static final int LUMP_HTIGHT_SPACE = 20;
    private static final int LUMP_MAX_HEIGHT = 360;
    private static final int LUMP_MIN_HEIGHT = 30;
    private static final int LUMP_SIZE = 40;
    private static final int LUMP_SPACE = 10;
    private static final int LUMP_WIDTH = 30;
    private static final float SCALE = 15.0f;
    private static final String TAG = "AudioView";
    private int heightSize;
    private Paint lumpPaint;
    private byte[] waveData;
    private int widthSize;

    public enum ShowStyle {
        STYLE_HOLLOW_LUMP,
        STYLE_WAVE,
        STYLE_NOTHING
    }

    private int getTopIndex(float f) {
        if (f >= 330.0f) {
            return 0;
        }
        if (f >= 280.0f) {
            return 1;
        }
        if (f >= 240.0f) {
            return 2;
        }
        if (f >= 200.0f) {
            return 3;
        }
        if (f >= 160.0f) {
            return 4;
        }
        if (f >= 120.0f) {
            return 5;
        }
        if (f >= 80.0f) {
            return 6;
        }
        if (f >= 40.0f) {
            return 7;
        }
        return f >= 0.0f ? 8 : 1;
    }

    public void setStyle(ShowStyle showStyle, ShowStyle showStyle2) {
    }

    public AudioView(Context context) {
        super(context);
        init();
    }

    public AudioView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        init();
    }

    public AudioView(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        init();
    }

    private void init() {
        Paint paint = new Paint();
        this.lumpPaint = paint;
        paint.setAntiAlias(true);
        this.lumpPaint.setColor(LUMP_COLOR);
        this.lumpPaint.setStrokeWidth(2.0f);
        this.lumpPaint.setStyle(Paint.Style.FILL);
    }

    public void setWaveData(byte[] bArr) {
        this.waveData = bArr;
        Log.e(TAG, "setWaveData: " + Arrays.toString(this.waveData));
        invalidate();
    }

    @Override // android.view.View
    protected void onDraw(Canvas canvas) {
        super.onDraw(canvas);
        int measuredWidth = getMeasuredWidth();
        Log.e(TAG, "init  width: " + getWidth() + " height:" + getHeight() + " width1:" + measuredWidth);
        for (int i = 0; i < 24; i++) {
            if (this.waveData == null) {
                Log.e(TAG, "onDraw: " + this.waveData);
            } else {
                drawLump1(canvas, i, false);
            }
        }
    }

    private void drawLump1(Canvas canvas, int i, boolean z) {
        Log.e(TAG, "drawLump1:绘制矩形条 reversal:" + z);
        float f = 360.0f - ((((this.waveData[i] / 14) * SCALE) + 30.0f) * (z ? -1 : 1));
        int i2 = i * 40;
        Log.e(TAG, "drawLump1: left: " + i2 + " top:" + f + " right:" + i2 + "30 bottom:360");
        int topIndex = getTopIndex(f);
        Log.e(TAG, "topindex: " + topIndex);
        setPillars(canvas, i, topIndex);
    }

    private void setPillars(Canvas canvas, int i, int i2) {
        int i3 = 0;
        while (i3 < i2) {
            int i4 = i3 + 1;
            int i5 = i3 * 20;
            canvas.drawRect(i * 40, (360 - (i4 * 20)) - i5, r2 + 30, (360 - i5) - i5, this.lumpPaint);
            i3 = i4;
        }
    }

    private static byte[] readyData(byte[] bArr) {
        byte[] bArr2 = new byte[24];
        for (int i = 0; i < 24; i++) {
            byte bAbs = (byte) Math.abs((int) bArr[i]);
            if (bAbs < 0) {
                bAbs = 127;
            }
            bArr2[i] = bAbs;
        }
        return bArr2;
    }

    private void drawLump(Canvas canvas, int i, boolean z) {
        float f = 360.0f - (((this.waveData[i] * SCALE) + 30.0f) * (z ? -1 : 1));
        int i2 = i * 40;
        Log.e(TAG, "drawLump1: left: " + i2 + " top:" + f + " right:" + i2 + "30 bottom:360");
        canvas.drawRect(i2, f, i2 + 30, 360.0f, this.lumpPaint);
    }

    @Override // android.view.View
    protected void onMeasure(int i, int i2) {
        this.widthSize = View.MeasureSpec.getSize(i);
        this.heightSize = View.MeasureSpec.getSize(i2);
        LogUtil.d("widthSize:" + this.widthSize + "  heightSize:" + this.heightSize);
        setMeasuredDimension(this.widthSize, this.heightSize);
    }

    @Override // android.view.View
    protected void onLayout(boolean z, int i, int i2, int i3, int i4) {
        super.onLayout(z, i, i2, i3, i4);
    }
}