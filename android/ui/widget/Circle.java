package cn.com.heaton.shiningmask.ui.widget;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.util.AttributeSet;
import android.util.TypedValue;
import android.view.View;

/* JADX INFO: loaded from: classes.dex */
public class Circle extends View {
    int color;
    Context mContext;
    Paint paint;

    public Circle(Context context) {
        super(context);
        this.color = Color.rgb(255, 255, 255);
        this.mContext = context;
    }

    public Circle(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.color = Color.rgb(255, 255, 255);
        this.mContext = context;
    }

    public Circle(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.color = Color.rgb(255, 255, 255);
        this.mContext = context;
    }

    private static int dp2px(Context context, float f) {
        return (int) TypedValue.applyDimension(1, f, context.getResources().getDisplayMetrics());
    }

    @Override // android.view.View
    public void draw(Canvas canvas) {
        super.draw(canvas);
        Paint paint = new Paint();
        this.paint = paint;
        paint.setColor(this.color);
        this.paint.setAntiAlias(true);
        this.paint.setStyle(Paint.Style.FILL_AND_STROKE);
        this.paint.setStrokeWidth(8.0f);
        canvas.drawCircle(getWidth() / 2, getHeight() / 2, dp2px(this.mContext, 10.0f), this.paint);
    }

    public void setColor(int i) {
        this.color = i;
        invalidate();
    }
}