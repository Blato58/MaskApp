package cn.com.heaton.shiningmask.base.recoder;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.util.AttributeSet;
import android.view.View;

/* JADX INFO: loaded from: classes.dex */
public class BaseVisualizerView extends View {
    private int Drawtype;
    private byte[] mBytes;
    private Paint mForePaint;
    private float[] mPoints;
    private int mSpectrumNum;
    public final int typeFFT;
    public final int typeWave;

    public BaseVisualizerView(Context context) {
        super(context);
        this.mForePaint = new Paint();
        this.mSpectrumNum = 64;
        this.typeFFT = 0;
        this.typeWave = 1;
        this.Drawtype = 0;
        init();
    }

    public BaseVisualizerView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.mForePaint = new Paint();
        this.mSpectrumNum = 64;
        this.typeFFT = 0;
        this.typeWave = 1;
        this.Drawtype = 0;
        init();
    }

    public BaseVisualizerView(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.mForePaint = new Paint();
        this.mSpectrumNum = 64;
        this.typeFFT = 0;
        this.typeWave = 1;
        this.Drawtype = 0;
        init();
    }

    public void setPaint(Paint paint) {
        this.mForePaint = new Paint(paint);
    }

    public void setPaintStock(float f) {
        this.mForePaint.setStrokeWidth(f);
    }

    private void init() {
        this.mBytes = null;
        this.mForePaint.setStrokeWidth(8.0f);
        this.mForePaint.setAntiAlias(true);
        this.mForePaint.setColor(-1);
    }

    public byte updateVisualizer(short[] sArr, short[] sArr2) {
        this.Drawtype = 0;
        int length = sArr.length;
        int i = this.mSpectrumNum;
        if (length != i * 2 || sArr2.length != i * 2 || sArr.length != sArr2.length) {
            return (byte) 0;
        }
        byte[] bArr = new byte[i];
        long j = 0;
        for (int i2 = 0; i2 < this.mSpectrumNum; i2++) {
            short s = sArr[i2];
            short s2 = sArr2[i2];
            byte bSqrt = (byte) (Math.sqrt((s * s) + (s2 * s2)) / 4.0d);
            bArr[i2] = bSqrt;
            j += (long) bSqrt;
        }
        this.mBytes = bArr;
        invalidate();
        return (byte) (j / ((long) i));
    }

    public void updateVisualizer(byte[] bArr) {
        this.Drawtype = 0;
        byte[] bArr2 = new byte[(bArr.length / 2) + 1];
        bArr2[0] = (byte) Math.abs((int) bArr[0]);
        int i = 2;
        for (int i2 = 1; i2 < this.mSpectrumNum; i2++) {
            bArr2[i2] = (byte) Math.hypot(bArr[i], bArr[i + 1]);
            i += 2;
        }
        this.mBytes = bArr2;
        invalidate();
    }

    public void updateVisualizer(short[] sArr) {
        this.Drawtype = 1;
        int width = getWidth();
        int height = getHeight() / 2;
        byte[] bArr = new byte[width];
        int i = 0;
        for (int i2 = 0; i2 < width; i2++) {
            bArr[i2] = (byte) ((((double) sArr[i]) / 16392.0d) * ((double) height));
            i++;
            if (i >= sArr.length) {
                i = 0;
            }
        }
        this.mBytes = bArr;
        invalidate();
    }

    @Override // android.view.View
    protected void onDraw(Canvas canvas) {
        super.onDraw(canvas);
        byte[] bArr = this.mBytes;
        if (bArr == null || bArr.length == 0) {
            return;
        }
        float[] fArr = this.mPoints;
        if (fArr == null || fArr.length < bArr.length * 4) {
            this.mPoints = new float[bArr.length * 4];
        }
        int i = this.Drawtype;
        int i2 = 0;
        if (i == 0) {
            float width = getWidth() / this.mSpectrumNum;
            int height = getHeight();
            this.mForePaint.setStrokeWidth((width * 3.0f) / 4.0f);
            while (i2 < this.mSpectrumNum) {
                byte[] bArr2 = this.mBytes;
                if (bArr2[i2] < 0) {
                    bArr2[i2] = 127;
                }
                int width2 = ((getWidth() * i2) / this.mSpectrumNum) + ((getWidth() / this.mSpectrumNum) / 2);
                float[] fArr2 = this.mPoints;
                int i3 = i2 * 4;
                float f = width2;
                fArr2[i3] = f;
                float f2 = height;
                fArr2[i3 + 1] = f2;
                fArr2[i3 + 2] = f;
                fArr2[i3 + 3] = f2 - ((f2 / 127.0f) * this.mBytes[i2]);
                i2++;
            }
        } else if (i == 1) {
            while (i2 < getWidth()) {
                float[] fArr3 = this.mPoints;
                int i4 = i2 * 4;
                float f3 = i2;
                fArr3[i4] = f3;
                fArr3[i4 + 1] = getHeight() / 2;
                float[] fArr4 = this.mPoints;
                fArr4[i4 + 2] = f3;
                fArr4[i4 + 3] = (getHeight() / 2) + this.mBytes[i2];
                i2++;
            }
        }
        canvas.drawLines(this.mPoints, this.mForePaint);
    }
}