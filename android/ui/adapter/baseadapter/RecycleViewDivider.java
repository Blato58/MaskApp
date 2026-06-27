package cn.com.heaton.shiningmask.ui.adapter.baseadapter;

import android.R;
import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Rect;
import android.graphics.drawable.Drawable;
import android.view.View;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.RecyclerView;

/* JADX INFO: loaded from: classes.dex */
public class RecycleViewDivider extends RecyclerView.ItemDecoration {
    private static final int[] ATTRS = {R.attr.listDivider};
    private Drawable mDivider;
    private int mDividerHeight;
    private int mOrientation;
    private Paint mPaint;

    public RecycleViewDivider(Context context, int i) {
        this.mDividerHeight = 2;
        if (i != 1 && i != 0) {
            throw new IllegalArgumentException("请输入正确的参数！");
        }
        this.mOrientation = i;
        TypedArray typedArrayObtainStyledAttributes = context.obtainStyledAttributes(ATTRS);
        this.mDivider = typedArrayObtainStyledAttributes.getDrawable(0);
        typedArrayObtainStyledAttributes.recycle();
    }

    public RecycleViewDivider(Context context, int i, int i2) {
        this(context, i);
        Drawable drawable = ContextCompat.getDrawable(context, i2);
        this.mDivider = drawable;
        this.mDividerHeight = drawable.getIntrinsicHeight();
    }

    public RecycleViewDivider(Context context, int i, int i2, int i3) {
        this(context, i);
        this.mDividerHeight = i2;
        Paint paint = new Paint(1);
        this.mPaint = paint;
        paint.setColor(i3);
        this.mPaint.setStyle(Paint.Style.FILL);
    }

    @Override // androidx.recyclerview.widget.RecyclerView.ItemDecoration
    public void getItemOffsets(Rect rect, View view, RecyclerView recyclerView, RecyclerView.State state) {
        super.getItemOffsets(rect, view, recyclerView, state);
        rect.set(0, 0, 0, this.mDividerHeight);
    }

    @Override // androidx.recyclerview.widget.RecyclerView.ItemDecoration
    public void onDraw(Canvas canvas, RecyclerView recyclerView, RecyclerView.State state) {
        super.onDraw(canvas, recyclerView, state);
        if (this.mOrientation == 1) {
            drawVertical(canvas, recyclerView);
        } else {
            drawHorizontal(canvas, recyclerView);
        }
    }

    private void drawHorizontal(Canvas canvas, RecyclerView recyclerView) {
        Canvas canvas2;
        int paddingLeft = recyclerView.getPaddingLeft();
        int measuredWidth = recyclerView.getMeasuredWidth() - recyclerView.getPaddingRight();
        int childCount = recyclerView.getChildCount();
        int i = 0;
        while (i < childCount) {
            View childAt = recyclerView.getChildAt(i);
            int bottom = childAt.getBottom() + ((RecyclerView.LayoutParams) childAt.getLayoutParams()).bottomMargin;
            int i2 = this.mDividerHeight + bottom;
            Drawable drawable = this.mDivider;
            if (drawable != null) {
                drawable.setBounds(paddingLeft, bottom, measuredWidth, i2);
                this.mDivider.draw(canvas);
            }
            Paint paint = this.mPaint;
            if (paint != null) {
                canvas2 = canvas;
                canvas2.drawRect(paddingLeft, bottom, measuredWidth, i2, paint);
            } else {
                canvas2 = canvas;
            }
            i++;
            canvas = canvas2;
        }
    }

    private void drawVertical(Canvas canvas, RecyclerView recyclerView) {
        Canvas canvas2;
        int paddingTop = recyclerView.getPaddingTop();
        int measuredHeight = recyclerView.getMeasuredHeight() - recyclerView.getPaddingBottom();
        int childCount = recyclerView.getChildCount();
        int i = 0;
        while (i < childCount) {
            View childAt = recyclerView.getChildAt(i);
            int right = childAt.getRight() + ((RecyclerView.LayoutParams) childAt.getLayoutParams()).rightMargin;
            int i2 = this.mDividerHeight + right;
            Drawable drawable = this.mDivider;
            if (drawable != null) {
                drawable.setBounds(right, paddingTop, i2, measuredHeight);
                this.mDivider.draw(canvas);
            }
            Paint paint = this.mPaint;
            if (paint != null) {
                canvas2 = canvas;
                canvas2.drawRect(right, paddingTop, i2, measuredHeight, paint);
            } else {
                canvas2 = canvas;
            }
            i++;
            canvas = canvas2;
        }
    }
}