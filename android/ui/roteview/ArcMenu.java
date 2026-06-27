package cn.com.heaton.shiningmask.ui.roteview;

import android.animation.Animator;
import android.animation.ValueAnimator;
import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Color;
import android.graphics.drawable.GradientDrawable;
import android.util.AttributeSet;
import android.view.View;
import android.view.ViewGroup;
import android.view.animation.AnticipateOvershootInterpolator;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public class ArcMenu extends ViewGroup implements View.OnClickListener {
    private final int CLOSE;
    private final int OPEN;
    private String[] colorArr;
    private float mAngle;
    private int mCurrentStatus;
    private View mMainButton;
    private OnMenuItemClickListener mMenuItemClickListener;
    private int mRadius;

    public interface OnMenuItemClickListener {
        void onClick(View view, int i);
    }

    public void setOnMenuItemClickListener(OnMenuItemClickListener onMenuItemClickListener) {
        this.mMenuItemClickListener = onMenuItemClickListener;
    }

    public ArcMenu(Context context) {
        this(context, null);
    }

    public ArcMenu(Context context, AttributeSet attributeSet) {
        this(context, attributeSet, 0);
    }

    public ArcMenu(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.CLOSE = 0;
        this.OPEN = 1;
        this.colorArr = new String[]{"#13A1EA", "#B024F8", "#F68929", "#F8248D", "#F8B024"};
        this.mCurrentStatus = 0;
        TypedArray typedArrayObtainStyledAttributes = context.getTheme().obtainStyledAttributes(attributeSet, R.styleable.ArcMenu, i, 0);
        this.mAngle = (float) ((((double) typedArrayObtainStyledAttributes.getFloat(R.styleable.ArcMenu_ang, 45.0f)) * 3.141592653589793d) / 180.0d);
        typedArrayObtainStyledAttributes.recycle();
    }

    @Override // android.view.View
    protected void onMeasure(int i, int i2) {
        int childCount = getChildCount();
        for (int i3 = 0; i3 < childCount; i3++) {
            measureChild(getChildAt(i3), i, i2);
        }
        super.onMeasure(i, i2);
    }

    @Override // android.view.ViewGroup, android.view.View
    protected void onLayout(boolean z, int i, int i2, int i3, int i4) {
        if (z) {
            int childCount = getChildCount();
            int i5 = childCount / 2;
            View childAt = getChildAt(i5);
            this.mMainButton = childAt;
            childAt.setOnClickListener(this);
            for (int i6 = 0; i6 < childCount; i6++) {
                View childAt2 = getChildAt(i6);
                if (!childAt2.equals(this.mMainButton)) {
                    childAt2.setVisibility(8);
                }
                float f = childCount - 1;
                float f2 = i6 - i5;
                int iSin = (((int) (((double) this.mRadius) * Math.sin((this.mAngle / f) * f2))) + (getMeasuredWidth() / 2)) - (childAt2.getMeasuredWidth() / 2);
                int iCos = (int) (((double) this.mRadius) * Math.cos((this.mAngle / f) * f2));
                childAt2.layout(iSin, iCos, childAt2.getMeasuredWidth() + iSin, childAt2.getMeasuredHeight() + iCos);
                GradientDrawable gradientDrawable = (GradientDrawable) childAt2.getBackground();
                String[] strArr = this.colorArr;
                gradientDrawable.setColor(Color.parseColor(strArr[i6 % strArr.length]));
            }
        }
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        toggleMenu(300);
    }

    private void toggleMenu(int i) {
        ValueAnimator valueAnimatorOfObject;
        int childCount = getChildCount();
        for (final int i2 = 0; i2 < childCount; i2++) {
            if (i2 != childCount / 2) {
                final View childAt = getChildAt(i2);
                childAt.setVisibility(0);
                float f = (this.mAngle / (childCount - 1)) * (i2 - r3);
                if (this.mCurrentStatus == 0) {
                    valueAnimatorOfObject = ValueAnimator.ofObject(new AngleEvaluator(), Float.valueOf(0.0f), Float.valueOf(f));
                } else {
                    valueAnimatorOfObject = ValueAnimator.ofObject(new AngleEvaluator(), Float.valueOf(f), Float.valueOf(0.0f));
                    valueAnimatorOfObject.addListener(new Animator.AnimatorListener() { // from class: cn.com.heaton.shiningmask.ui.roteview.ArcMenu.1
                        @Override // android.animation.Animator.AnimatorListener
                        public void onAnimationCancel(Animator animator) {
                        }

                        @Override // android.animation.Animator.AnimatorListener
                        public void onAnimationRepeat(Animator animator) {
                        }

                        @Override // android.animation.Animator.AnimatorListener
                        public void onAnimationStart(Animator animator) {
                        }

                        @Override // android.animation.Animator.AnimatorListener
                        public void onAnimationEnd(Animator animator) {
                            childAt.setVisibility(8);
                        }
                    });
                }
                valueAnimatorOfObject.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() { // from class: cn.com.heaton.shiningmask.ui.roteview.ArcMenu.2
                    @Override // android.animation.ValueAnimator.AnimatorUpdateListener
                    public void onAnimationUpdate(ValueAnimator valueAnimator) {
                        double dFloatValue = ((Float) valueAnimator.getAnimatedValue()).floatValue();
                        int iSin = (((int) (((double) ArcMenu.this.mRadius) * Math.sin(dFloatValue))) + (ArcMenu.this.getMeasuredWidth() / 2)) - (childAt.getMeasuredWidth() / 2);
                        int iCos = (int) (((double) ArcMenu.this.mRadius) * Math.cos(dFloatValue));
                        childAt.layout(iSin, iCos, childAt.getMeasuredWidth() + iSin, childAt.getMeasuredHeight() + iCos);
                    }
                });
                valueAnimatorOfObject.setInterpolator(new AnticipateOvershootInterpolator());
                valueAnimatorOfObject.setDuration(i);
                valueAnimatorOfObject.start();
                childAt.setOnClickListener(new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.roteview.ArcMenu.3
                    @Override // android.view.View.OnClickListener
                    public void onClick(View view) {
                        if (ArcMenu.this.mMenuItemClickListener != null) {
                            ArcMenu.this.mMenuItemClickListener.onClick(childAt, i2);
                        }
                    }
                });
            }
        }
        changeStatus();
    }

    private void changeStatus() {
        this.mCurrentStatus = this.mCurrentStatus == 0 ? 1 : 0;
    }

    public void setColorArr(String[] strArr) {
        this.colorArr = strArr;
    }
}