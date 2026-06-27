package cn.com.heaton.shiningmask.ui.widget;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.RectF;
import android.util.AttributeSet;
import android.view.View;

/* JADX INFO: loaded from: classes.dex */
public class RectView extends View {
    private int curColor;
    Paint paint;

    public RectView(Context context) {
        super(context);
        this.paint = new Paint(1);
        this.curColor = -16711936;
    }

    public RectView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.paint = new Paint(1);
        this.curColor = -16711936;
    }

    public RectView(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.paint = new Paint(1);
        this.curColor = -16711936;
    }

    @Override // android.view.View
    public void draw(Canvas canvas) {
        super.draw(canvas);
        this.paint.setColor(this.curColor);
        this.paint.setStrokeWidth(1.5f);
        this.paint.setStyle(Paint.Style.FILL);
        canvas.drawRoundRect(new RectF(0.0f, 0.0f, canvas.getWidth(), getHeight()), 10.0f, 10.0f, this.paint);
    }

    public void setViewBackground(int i) {
        this.curColor = i;
        invalidate();
    }
}