package cn.com.heaton.shiningmask.ui.widget;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Rect;
import android.util.AttributeSet;
import android.view.View;

/* JADX INFO: loaded from: classes.dex */
public class LedItemRhyView extends View {
    private static final String TAG = "LedItemView";
    private int columnNumber;
    private boolean isChecked;
    private Paint paint;
    private int rowNumber;
    private int viewNumber;

    public LedItemRhyView(Context context) {
        super(context);
        init(0, 0.0f);
    }

    public LedItemRhyView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        init(0, 0.0f);
    }

    public LedItemRhyView(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        init(0, 0.0f);
    }

    private void init(int i, float f) {
        Paint paint = new Paint();
        this.paint = paint;
        paint.setAntiAlias(true);
        this.paint.setColor(i);
        initPaint();
    }

    @Override // android.view.View
    protected void onDraw(Canvas canvas) {
        super.onDraw(canvas);
        int width = (int) (((double) getWidth()) * 0.9d);
        int height = (int) (((double) getHeight()) * 0.9d);
        getWidth();
        getHeight();
        canvas.drawRect(new Rect(width, height, 0, 0), this.paint);
    }

    public int getViewNumber() {
        return this.viewNumber;
    }

    public void setViewNumber(int i) {
        this.viewNumber = i;
    }

    public int getColumnNumber() {
        return this.columnNumber;
    }

    public void setColumnNumber(int i) {
        this.columnNumber = i;
    }

    public int getRowNumber() {
        return this.rowNumber;
    }

    public void setRowNumber(int i) {
        this.rowNumber = i;
    }

    public boolean isChecked() {
        return this.isChecked;
    }

    public void setChecked(boolean z) {
        this.isChecked = z;
    }

    public void setPaint(int i) {
        this.paint.setColor(i);
        initPaint();
    }

    public void setPaint(int i, float f) {
        this.paint.setColor(i);
        initPaint();
    }

    private void initPaint() {
        if (this.isChecked) {
            this.paint.setStyle(Paint.Style.FILL);
        } else {
            this.paint.setStyle(Paint.Style.FILL);
            this.paint.setStrokeWidth(0.0f);
        }
    }
}