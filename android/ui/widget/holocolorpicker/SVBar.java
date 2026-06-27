package cn.com.heaton.shiningmask.ui.widget.holocolorpicker;

import android.content.Context;
import android.content.res.Resources;
import android.content.res.TypedArray;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.LinearGradient;
import android.graphics.Paint;
import android.graphics.RectF;
import android.graphics.Shader;
import android.os.Bundle;
import android.os.Parcelable;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;
import androidx.core.view.ViewCompat;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public class SVBar extends View {
    private static final boolean ORIENTATION_DEFAULT = true;
    private static final boolean ORIENTATION_HORIZONTAL = true;
    private static final boolean ORIENTATION_VERTICAL = false;
    private static final String STATE_COLOR = "color";
    private static final String STATE_ORIENTATION = "orientation";
    private static final String STATE_PARENT = "parent";
    private static final String STATE_SATURATION = "saturation";
    private static final String STATE_VALUE = "value";
    private int mBarLength;
    private Paint mBarPaint;
    private Paint mBarPointerHaloPaint;
    private int mBarPointerHaloRadius;
    private Paint mBarPointerPaint;
    private int mBarPointerPosition;
    private int mBarPointerRadius;
    private RectF mBarRect;
    private int mBarThickness;
    private int mColor;
    private float[] mHSVColor;
    private boolean mIsMovingPointer;
    private boolean mOrientation;
    private ColorPicker mPicker;
    private float mPosToSVFactor;
    private int mPreferredBarLength;
    private float mSVToPosFactor;
    private Shader shader;

    public SVBar(Context context) {
        super(context);
        this.mBarRect = new RectF();
        this.mHSVColor = new float[3];
        this.mPicker = null;
        init(null, 0);
    }

    public SVBar(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.mBarRect = new RectF();
        this.mHSVColor = new float[3];
        this.mPicker = null;
        init(attributeSet, 0);
    }

    public SVBar(Context context, AttributeSet attributeSet, int i) {
        super(context, attributeSet, i);
        this.mBarRect = new RectF();
        this.mHSVColor = new float[3];
        this.mPicker = null;
        init(attributeSet, i);
    }

    private void init(AttributeSet attributeSet, int i) {
        TypedArray typedArrayObtainStyledAttributes = getContext().obtainStyledAttributes(attributeSet, R.styleable.ColorBars, i, 0);
        Resources resources = getContext().getResources();
        this.mBarThickness = typedArrayObtainStyledAttributes.getDimensionPixelSize(R.styleable.ColorBars_bar_thickness, resources.getDimensionPixelSize(R.dimen.bar_thickness));
        int dimensionPixelSize = typedArrayObtainStyledAttributes.getDimensionPixelSize(R.styleable.ColorBars_bar_length, resources.getDimensionPixelSize(R.dimen.bar_length));
        this.mBarLength = dimensionPixelSize;
        this.mPreferredBarLength = dimensionPixelSize;
        this.mBarPointerRadius = typedArrayObtainStyledAttributes.getDimensionPixelSize(R.styleable.ColorBars_bar_pointer_radius, resources.getDimensionPixelSize(R.dimen.bar_pointer_radius));
        this.mBarPointerHaloRadius = typedArrayObtainStyledAttributes.getDimensionPixelSize(R.styleable.ColorBars_bar_pointer_halo_radius, resources.getDimensionPixelSize(R.dimen.bar_pointer_halo_radius));
        this.mOrientation = typedArrayObtainStyledAttributes.getBoolean(R.styleable.ColorBars_bar_orientation_horizontal, true);
        typedArrayObtainStyledAttributes.recycle();
        Paint paint = new Paint(1);
        this.mBarPaint = paint;
        paint.setShader(this.shader);
        this.mBarPointerPosition = (this.mBarLength / 2) + this.mBarPointerHaloRadius;
        Paint paint2 = new Paint(1);
        this.mBarPointerHaloPaint = paint2;
        paint2.setColor(ViewCompat.MEASURED_STATE_MASK);
        this.mBarPointerHaloPaint.setAlpha(80);
        Paint paint3 = new Paint(1);
        this.mBarPointerPaint = paint3;
        paint3.setColor(-8257792);
        int i2 = this.mBarLength;
        this.mPosToSVFactor = 1.0f / (i2 / 2.0f);
        this.mSVToPosFactor = (i2 / 2.0f) / 1.0f;
    }

    @Override // android.view.View
    protected void onMeasure(int i, int i2) {
        int iMin = this.mPreferredBarLength + (this.mBarPointerHaloRadius * 2);
        if (!this.mOrientation) {
            i = i2;
        }
        int mode = View.MeasureSpec.getMode(i);
        int size = View.MeasureSpec.getSize(i);
        if (mode == 1073741824) {
            iMin = size;
        } else if (mode == Integer.MIN_VALUE) {
            iMin = Math.min(iMin, size);
        }
        int i3 = this.mBarPointerHaloRadius * 2;
        int i4 = iMin - i3;
        this.mBarLength = i4;
        if (!this.mOrientation) {
            setMeasuredDimension(i3, i4 + i3);
        } else {
            setMeasuredDimension(i4 + i3, i3);
        }
    }

    @Override // android.view.View
    protected void onSizeChanged(int i, int i2, int i3, int i4) {
        int i5;
        int i6;
        super.onSizeChanged(i, i2, i3, i4);
        if (this.mOrientation) {
            int i7 = this.mBarLength;
            int i8 = this.mBarPointerHaloRadius;
            i5 = i7 + i8;
            i6 = this.mBarThickness;
            this.mBarLength = i - (i8 * 2);
            this.mBarRect.set(i8, i8 - (i6 / 2), r12 + i8, i8 + (i6 / 2));
        } else {
            int i9 = this.mBarThickness;
            int i10 = this.mBarLength;
            int i11 = this.mBarPointerHaloRadius;
            this.mBarLength = i2 - (i11 * 2);
            this.mBarRect.set(i11 - (i9 / 2), i11, (i9 / 2) + i11, r13 + i11);
            i5 = i9;
            i6 = i10 + i11;
        }
        if (!isInEditMode()) {
            this.shader = new LinearGradient(this.mBarPointerHaloRadius, 0.0f, i5, i6, new int[]{-1, Color.HSVToColor(this.mHSVColor), ViewCompat.MEASURED_STATE_MASK}, (float[]) null, Shader.TileMode.CLAMP);
        } else {
            this.shader = new LinearGradient(this.mBarPointerHaloRadius, 0.0f, i5, i6, new int[]{-1, -8257792, ViewCompat.MEASURED_STATE_MASK}, (float[]) null, Shader.TileMode.CLAMP);
            Color.colorToHSV(-8257792, this.mHSVColor);
        }
        this.mBarPaint.setShader(this.shader);
        int i12 = this.mBarLength;
        this.mPosToSVFactor = 1.0f / (i12 / 2.0f);
        this.mSVToPosFactor = (i12 / 2.0f) / 1.0f;
        float[] fArr = new float[3];
        Color.colorToHSV(this.mColor, fArr);
        float f = fArr[1];
        float f2 = fArr[2];
        if (f < f2) {
            this.mBarPointerPosition = Math.round((this.mSVToPosFactor * f) + this.mBarPointerHaloRadius);
        } else {
            this.mBarPointerPosition = Math.round((this.mSVToPosFactor * (1.0f - f2)) + this.mBarPointerHaloRadius + (this.mBarLength / 2));
        }
        if (isInEditMode()) {
            this.mBarPointerPosition = (this.mBarLength / 2) + this.mBarPointerHaloRadius;
        }
    }

    @Override // android.view.View
    protected void onDraw(Canvas canvas) {
        int i;
        int i2;
        canvas.drawRect(this.mBarRect, this.mBarPaint);
        if (this.mOrientation) {
            i = this.mBarPointerPosition;
            i2 = this.mBarPointerHaloRadius;
        } else {
            i = this.mBarPointerHaloRadius;
            i2 = this.mBarPointerPosition;
        }
        float f = i;
        float f2 = i2;
        canvas.drawCircle(f, f2, this.mBarPointerHaloRadius, this.mBarPointerHaloPaint);
        canvas.drawCircle(f, f2, this.mBarPointerRadius, this.mBarPointerPaint);
    }

    @Override // android.view.View
    public boolean onTouchEvent(MotionEvent motionEvent) {
        float y;
        getParent().requestDisallowInterceptTouchEvent(true);
        if (this.mOrientation) {
            y = motionEvent.getX();
        } else {
            y = motionEvent.getY();
        }
        int action = motionEvent.getAction();
        if (action == 0) {
            this.mIsMovingPointer = true;
            if (y >= this.mBarPointerHaloRadius && y <= r5 + this.mBarLength) {
                this.mBarPointerPosition = Math.round(y);
                calculateColor(Math.round(y));
                this.mBarPointerPaint.setColor(this.mColor);
                invalidate();
            }
        } else if (action != 1) {
            if (action == 2 && this.mIsMovingPointer) {
                int i = this.mBarPointerHaloRadius;
                if (y >= i && y <= this.mBarLength + i) {
                    this.mBarPointerPosition = Math.round(y);
                    calculateColor(Math.round(y));
                    this.mBarPointerPaint.setColor(this.mColor);
                    ColorPicker colorPicker = this.mPicker;
                    if (colorPicker != null) {
                        colorPicker.setNewCenterColor(this.mColor);
                        this.mPicker.changeOpacityBarColor(this.mColor);
                    }
                    invalidate();
                } else if (y < i) {
                    this.mBarPointerPosition = i;
                    this.mColor = -1;
                    this.mBarPointerPaint.setColor(-1);
                    ColorPicker colorPicker2 = this.mPicker;
                    if (colorPicker2 != null) {
                        colorPicker2.setNewCenterColor(this.mColor);
                        this.mPicker.changeOpacityBarColor(this.mColor);
                    }
                    invalidate();
                } else {
                    int i2 = this.mBarLength;
                    if (y > i + i2) {
                        this.mBarPointerPosition = i + i2;
                        this.mColor = ViewCompat.MEASURED_STATE_MASK;
                        this.mBarPointerPaint.setColor(ViewCompat.MEASURED_STATE_MASK);
                        ColorPicker colorPicker3 = this.mPicker;
                        if (colorPicker3 != null) {
                            colorPicker3.setNewCenterColor(this.mColor);
                            this.mPicker.changeOpacityBarColor(this.mColor);
                        }
                        invalidate();
                    }
                }
            }
        } else {
            this.mIsMovingPointer = false;
        }
        return true;
    }

    public void setSaturation(float f) {
        int iRound = Math.round((this.mSVToPosFactor * f) + this.mBarPointerHaloRadius);
        this.mBarPointerPosition = iRound;
        calculateColor(iRound);
        this.mBarPointerPaint.setColor(this.mColor);
        ColorPicker colorPicker = this.mPicker;
        if (colorPicker != null) {
            colorPicker.setNewCenterColor(this.mColor);
            this.mPicker.changeOpacityBarColor(this.mColor);
        }
        invalidate();
    }

    public void setValue(float f) {
        int iRound = Math.round((this.mSVToPosFactor * (1.0f - f)) + this.mBarPointerHaloRadius + (this.mBarLength / 2));
        this.mBarPointerPosition = iRound;
        calculateColor(iRound);
        this.mBarPointerPaint.setColor(this.mColor);
        ColorPicker colorPicker = this.mPicker;
        if (colorPicker != null) {
            colorPicker.setNewCenterColor(this.mColor);
            this.mPicker.changeOpacityBarColor(this.mColor);
        }
        invalidate();
    }

    public void setColor(int i) {
        int i2;
        int i3;
        if (this.mOrientation) {
            i2 = this.mBarLength + this.mBarPointerHaloRadius;
            i3 = this.mBarThickness;
        } else {
            i2 = this.mBarThickness;
            i3 = this.mBarLength + this.mBarPointerHaloRadius;
        }
        Color.colorToHSV(i, this.mHSVColor);
        LinearGradient linearGradient = new LinearGradient(this.mBarPointerHaloRadius, 0.0f, i2, i3, new int[]{-1, i, ViewCompat.MEASURED_STATE_MASK}, (float[]) null, Shader.TileMode.CLAMP);
        this.shader = linearGradient;
        this.mBarPaint.setShader(linearGradient);
        calculateColor(this.mBarPointerPosition);
        this.mBarPointerPaint.setColor(this.mColor);
        ColorPicker colorPicker = this.mPicker;
        if (colorPicker != null) {
            colorPicker.setNewCenterColor(this.mColor);
            this.mPicker.changeOpacityBarColor(this.mColor);
        }
        invalidate();
    }

    private void calculateColor(int i) {
        int i2 = i - this.mBarPointerHaloRadius;
        int i3 = this.mBarLength;
        if (i2 > i3 / 2 && i2 < i3) {
            this.mColor = Color.HSVToColor(new float[]{this.mHSVColor[0], 1.0f, 1.0f - (this.mPosToSVFactor * (i2 - (i3 / 2)))});
            return;
        }
        if (i2 > 0 && i2 < i3) {
            this.mColor = Color.HSVToColor(new float[]{this.mHSVColor[0], this.mPosToSVFactor * i2, 1.0f});
            return;
        }
        if (i2 == i3 / 2) {
            this.mColor = Color.HSVToColor(new float[]{this.mHSVColor[0], 1.0f, 1.0f});
        } else if (i2 <= 0) {
            this.mColor = -1;
        } else if (i2 >= i3) {
            this.mColor = ViewCompat.MEASURED_STATE_MASK;
        }
    }

    public int getColor() {
        return this.mColor;
    }

    public void setColorPicker(ColorPicker colorPicker) {
        this.mPicker = colorPicker;
    }

    @Override // android.view.View
    protected Parcelable onSaveInstanceState() {
        Parcelable parcelableOnSaveInstanceState = super.onSaveInstanceState();
        Bundle bundle = new Bundle();
        bundle.putParcelable(STATE_PARENT, parcelableOnSaveInstanceState);
        bundle.putFloatArray("color", this.mHSVColor);
        float[] fArr = new float[3];
        Color.colorToHSV(this.mColor, fArr);
        float f = fArr[1];
        float f2 = fArr[2];
        if (f < f2) {
            bundle.putFloat(STATE_SATURATION, f);
            return bundle;
        }
        bundle.putFloat(STATE_VALUE, f2);
        return bundle;
    }

    @Override // android.view.View
    protected void onRestoreInstanceState(Parcelable parcelable) {
        Bundle bundle = (Bundle) parcelable;
        super.onRestoreInstanceState(bundle.getParcelable(STATE_PARENT));
        setColor(Color.HSVToColor(bundle.getFloatArray("color")));
        if (bundle.containsKey(STATE_SATURATION)) {
            setSaturation(bundle.getFloat(STATE_SATURATION));
        } else {
            setValue(bundle.getFloat(STATE_VALUE));
        }
    }
}