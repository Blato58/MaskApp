package cn.com.heaton.shiningmask.ui.widget.camera;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Rect;
import android.hardware.Camera;
import android.util.AttributeSet;
import android.util.Log;
import android.view.WindowManager;
import androidx.appcompat.widget.AppCompatImageView;
import com.cdbwsoft.library.audio.AudioPlayer;
import java.util.ArrayList;

/* JADX INFO: loaded from: classes.dex */
public class OverCameraView extends AppCompatImageView {
    private Context context;
    private boolean isFoucuing;
    private Paint touchFocusPaint;
    private Rect touchFocusRect;

    public OverCameraView(Context context) {
        this(context, null, 0);
    }

    public OverCameraView(Context context, AttributeSet attributeSet) {
        this(context, attributeSet, 0);
    }

    public OverCameraView(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        init(context);
    }

    private void init(Context context) {
        this.context = context;
        Paint paint = new Paint();
        this.touchFocusPaint = paint;
        paint.setColor(-16711936);
        this.touchFocusPaint.setStyle(Paint.Style.STROKE);
        this.touchFocusPaint.setStrokeWidth(3.0f);
    }

    public boolean isFoucuing() {
        return this.isFoucuing;
    }

    public void setFoucuing(boolean z) {
        this.isFoucuing = z;
    }

    public void setTouchFoucusRect(Camera camera, Camera.AutoFocusCallback autoFocusCallback, float f, float f2) {
        Rect rect = new Rect((int) (f - 100.0f), (int) (f2 - 100.0f), (int) (f + 100.0f), (int) (f2 + 100.0f));
        this.touchFocusRect = rect;
        int windowWidth = ((rect.left * AudioPlayer.HEADER_SAMPLE_RATE) / getWindowWidth(this.context)) - 1000;
        int windowHeight = ((this.touchFocusRect.top * AudioPlayer.HEADER_SAMPLE_RATE) / getWindowHeight(this.context)) - 1000;
        int windowWidth2 = ((this.touchFocusRect.right * AudioPlayer.HEADER_SAMPLE_RATE) / getWindowWidth(this.context)) - 1000;
        int windowHeight2 = ((this.touchFocusRect.bottom * AudioPlayer.HEADER_SAMPLE_RATE) / getWindowHeight(this.context)) - 1000;
        if (windowWidth < -1000) {
            windowWidth = -1000;
        }
        if (windowHeight < -1000) {
            windowHeight = -1000;
        }
        if (windowWidth2 > 1000) {
            windowWidth2 = 1000;
        }
        doTouchFocus(camera, autoFocusCallback, new Rect(windowWidth, windowHeight, windowWidth2, windowHeight2 <= 1000 ? windowHeight2 : 1000));
        postInvalidate();
    }

    public void doTouchFocus(Camera camera, Camera.AutoFocusCallback autoFocusCallback, Rect rect) {
        if (camera == null || this.isFoucuing) {
            return;
        }
        try {
            ArrayList arrayList = new ArrayList();
            arrayList.add(new Camera.Area(rect, 1000));
            Camera.Parameters parameters = camera.getParameters();
            parameters.setFocusAreas(arrayList);
            parameters.setMeteringAreas(arrayList);
            parameters.setFocusMode("auto");
            camera.cancelAutoFocus();
            camera.setParameters(parameters);
            camera.autoFocus(autoFocusCallback);
            this.isFoucuing = true;
        } catch (Exception e) {
            Log.e("设置相机参数异常", e.getMessage());
        }
    }

    public void disDrawTouchFocusRect() {
        this.touchFocusRect = null;
        postInvalidate();
    }

    @Override // android.widget.ImageView, android.view.View
    protected void onDraw(Canvas canvas) {
        drawTouchFocusRect(canvas);
        super.onDraw(canvas);
    }

    public static int getWindowHeight(Context context) {
        return ((WindowManager) context.getSystemService("window")).getDefaultDisplay().getHeight();
    }

    public static int getWindowWidth(Context context) {
        return ((WindowManager) context.getSystemService("window")).getDefaultDisplay().getWidth();
    }

    private void drawTouchFocusRect(Canvas canvas) {
        if (this.touchFocusRect != null) {
            canvas.drawRect(r0.left - 2, this.touchFocusRect.bottom, this.touchFocusRect.left + 20, this.touchFocusRect.bottom + 2, this.touchFocusPaint);
            canvas.drawRect(this.touchFocusRect.left - 2, this.touchFocusRect.bottom - 20, this.touchFocusRect.left, this.touchFocusRect.bottom, this.touchFocusPaint);
            canvas.drawRect(this.touchFocusRect.left - 2, this.touchFocusRect.top - 2, this.touchFocusRect.left + 20, this.touchFocusRect.top, this.touchFocusPaint);
            canvas.drawRect(this.touchFocusRect.left - 2, this.touchFocusRect.top, this.touchFocusRect.left, this.touchFocusRect.top + 20, this.touchFocusPaint);
            canvas.drawRect(this.touchFocusRect.right - 20, this.touchFocusRect.top - 2, this.touchFocusRect.right + 2, this.touchFocusRect.top, this.touchFocusPaint);
            canvas.drawRect(this.touchFocusRect.right, this.touchFocusRect.top, this.touchFocusRect.right + 2, this.touchFocusRect.top + 20, this.touchFocusPaint);
            canvas.drawRect(this.touchFocusRect.right - 20, this.touchFocusRect.bottom, this.touchFocusRect.right + 2, this.touchFocusRect.bottom + 2, this.touchFocusPaint);
            canvas.drawRect(this.touchFocusRect.right, this.touchFocusRect.bottom - 20, this.touchFocusRect.right + 2, this.touchFocusRect.bottom, this.touchFocusPaint);
        }
    }
}