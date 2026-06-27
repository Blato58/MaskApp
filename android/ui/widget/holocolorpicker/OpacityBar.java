package cn.com.heaton.shiningmask.ui.widget.holocolorpicker;

import android.content.Context;
import android.content.res.Resources;
import android.content.res.TypedArray;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.LinearGradient;
import android.graphics.Paint;
import android.graphics.Rect;
import android.graphics.RectF;
import android.graphics.Shader;
import android.os.Bundle;
import android.os.Parcelable;
import android.util.AttributeSet;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.view.WindowManager;
import androidx.core.view.ViewCompat;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public class OpacityBar extends View {
    public static final int FULL_OPACITY = 208;
    public static final int MIN_OPACITY = 47;
    private static final boolean ORIENTATION_DEFAULT = true;
    private static final boolean ORIENTATION_HORIZONTAL = true;
    private static final boolean ORIENTATION_VERTICAL = false;
    private static final String STATE_COLOR = "color";
    private static final String STATE_OPACITY = "opacity";
    private static final String STATE_ORIENTATION = "orientation";
    private static final String STATE_PARENT = "parent";
    private static final String TAG = "OpacityBar";
    private int mBarLength;
    private Paint mBarPaint;
    private Paint mBarPointerHaloPaint;
    private int mBarPointerHaloRadius;
    private int mBarPointerPosition;
    private int mBarPointerRadius;
    private RectF mBarRect;
    private int mBarThickness;
    private int mColor;
    private Rect mDestRect;
    private float[] mHSVColor;
    private boolean mIsMovingPointer;
    private float mOpacToPosFactor;
    private boolean mOrientation;
    private ColorPicker mPicker;
    private float mPosToOpacFactor;
    private int mPreferredBarLength;
    private int oldChangedListenerOpacity;
    private OnOpacityChangedListener onOpacityChangedListener;
    private Shader shader;

    public interface OnOpacityChangedListener {
        void onOpacityChanged(int i);
    }

    public int getMatchColor(int i) {
        return (i & ViewCompat.MEASURED_SIZE_MASK) | (((int) (((((i >> 24) & 255) / 255.0f) * 208.0f) + 47.0f)) << 24);
    }

    public void setOpacity(int i, boolean z) {
    }

    public void setOnOpacityChangedListener(OnOpacityChangedListener onOpacityChangedListener) {
        this.onOpacityChangedListener = onOpacityChangedListener;
    }

    public OnOpacityChangedListener getOnOpacityChangedListener() {
        return this.onOpacityChangedListener;
    }

    public OpacityBar(Context context) {
        super(context);
        this.mBarRect = new RectF();
        this.mHSVColor = new float[3];
        this.mPicker = null;
        init(null, 0);
    }

    public OpacityBar(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.mBarRect = new RectF();
        this.mHSVColor = new float[3];
        this.mPicker = null;
        init(attributeSet, 0);
    }

    public OpacityBar(Context context, AttributeSet attributeSet, int i) {
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
        this.mBarPointerPosition = this.mBarLength + this.mBarPointerHaloRadius;
        Paint paint2 = new Paint(1);
        this.mBarPointerHaloPaint = paint2;
        paint2.setColor(ViewCompat.MEASURED_STATE_MASK);
        this.mBarPointerHaloPaint.setAlpha(80);
        int i2 = this.mBarLength;
        this.mPosToOpacFactor = 208.0f / i2;
        this.mOpacToPosFactor = i2 / 208.0f;
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
            this.mBarRect.set(i8, i8 - (i6 / 2), r10 + i8, i8 + (i6 / 2));
        } else {
            int i9 = this.mBarThickness;
            int i10 = this.mBarLength;
            int i11 = this.mBarPointerHaloRadius;
            this.mBarLength = i2 - (i11 * 2);
            this.mBarRect.set(i11 - (i9 / 2), i11, (i9 / 2) + i11, r11 + i11);
            i5 = i9;
            i6 = i10 + i11;
        }
        if (!isInEditMode()) {
            this.shader = new LinearGradient(this.mBarPointerHaloRadius, 0.0f, i5, i6, new int[]{Color.HSVToColor(0, this.mHSVColor), Color.HSVToColor(255, this.mHSVColor)}, (float[]) null, Shader.TileMode.CLAMP);
        } else {
            this.shader = new LinearGradient(this.mBarPointerHaloRadius, 0.0f, i5, i6, new int[]{8519424, -8257792}, (float[]) null, Shader.TileMode.CLAMP);
            Color.colorToHSV(-8257792, this.mHSVColor);
        }
        this.mBarPaint.setShader(this.shader);
        int i12 = this.mBarLength;
        this.mPosToOpacFactor = 208.0f / i12;
        this.mOpacToPosFactor = i12 / 208.0f;
        Color.colorToHSV(this.mColor, new float[3]);
        if (!isInEditMode()) {
            this.mBarPointerPosition = Math.round((this.mOpacToPosFactor * Color.alpha(this.mColor)) + this.mBarPointerHaloRadius);
        } else {
            this.mBarPointerPosition = this.mBarLength + this.mBarPointerHaloRadius;
        }
    }

    @Override // android.view.View
    protected void onDraw(Canvas canvas) {
        int i;
        int i2;
        if (this.mOrientation) {
            i = this.mBarPointerPosition;
            i2 = this.mBarPointerHaloRadius;
        } else {
            i = this.mBarPointerHaloRadius;
            i2 = this.mBarPointerPosition;
        }
        WindowManager windowManager = (WindowManager) getContext().getSystemService("window");
        windowManager.getDefaultDisplay().getWidth();
        windowManager.getDefaultDisplay().getHeight();
        Bitmap bitmapDecodeResource = BitmapFactory.decodeResource(getContext().getResources(), R.mipmap.bar_pointer);
        Bitmap bitmapDecodeResource2 = BitmapFactory.decodeResource(getContext().getResources(), R.mipmap.progress_);
        canvas.drawBitmap(bitmapDecodeResource2, (Rect) null, new Rect(0, (bitmapDecodeResource2.getHeight() / 2) / 2, this.mPreferredBarLength, bitmapDecodeResource2.getHeight() + (bitmapDecodeResource2.getHeight() / 2)), (Paint) null);
        canvas.drawBitmap(bitmapDecodeResource, i - (bitmapDecodeResource.getWidth() / 2.0f), i2 - (bitmapDecodeResource.getHeight() / 2.0f), (Paint) null);
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
                invalidate();
            }
        } else if (action == 1) {
            this.mIsMovingPointer = false;
        } else if (action == 2) {
            if (this.mIsMovingPointer) {
                int i = this.mBarPointerHaloRadius;
                if (y >= i && y <= this.mBarLength + i) {
                    this.mBarPointerPosition = Math.round(y);
                    calculateColor(Math.round(y));
                    ColorPicker colorPicker = this.mPicker;
                    if (colorPicker != null) {
                        colorPicker.setColorAlpha(getOpacity());
                    }
                    invalidate();
                } else if (y < i) {
                    this.mBarPointerPosition = i;
                    ColorPicker colorPicker2 = this.mPicker;
                    if (colorPicker2 != null) {
                        colorPicker2.setColorAlpha(getOpacity());
                    }
                    invalidate();
                } else {
                    int i2 = this.mBarLength;
                    if (y > i + i2) {
                        this.mBarPointerPosition = i + i2;
                        this.mColor = Color.HSVToColor(this.mHSVColor);
                        ColorPicker colorPicker3 = this.mPicker;
                        if (colorPicker3 != null) {
                            colorPicker3.setColorAlpha(getOpacity());
                        }
                        invalidate();
                    }
                }
                Log.e(TAG, "onTouchEvent: mBarPointerPosition:" + this.mBarPointerPosition);
            }
            if (this.onOpacityChangedListener != null && this.oldChangedListenerOpacity != getOpacity()) {
                this.onOpacityChangedListener.onOpacityChanged(getOpacity());
                this.oldChangedListenerOpacity = getOpacity();
            }
        }
        return true;
    }

    public void setColor(int i) {
        int i2;
        int i3;
        int matchColor = getMatchColor(i);
        if (this.mOrientation) {
            i2 = this.mBarLength + this.mBarPointerHaloRadius;
            i3 = this.mBarThickness;
        } else {
            i2 = this.mBarThickness;
            i3 = this.mBarLength + this.mBarPointerHaloRadius;
        }
        Color.colorToHSV(matchColor, this.mHSVColor);
        LinearGradient linearGradient = new LinearGradient(this.mBarPointerHaloRadius, 0.0f, i2, i3, new int[]{Color.HSVToColor(0, this.mHSVColor), matchColor}, (float[]) null, Shader.TileMode.CLAMP);
        this.shader = linearGradient;
        this.mBarPaint.setShader(linearGradient);
        calculateColor(this.mBarPointerPosition);
        ColorPicker colorPicker = this.mPicker;
        if (colorPicker != null) {
            colorPicker.setNewCenterColor(this.mColor);
        }
        invalidate();
    }

    public void setOpacity(int i) {
        setOpacity(i, false);
    }

    public int getOpacity() {
        int iRound = Math.round(this.mPosToOpacFactor * (this.mBarPointerPosition - this.mBarPointerHaloRadius));
        if (iRound <= 0) {
            iRound = 0;
        }
        if (iRound >= 208) {
            iRound = 208;
        }
        return iRound + 47;
    }

    private void calculateColor(int i) {
        int i2 = i - this.mBarPointerHaloRadius;
        if (i2 < 0) {
            i2 = 0;
        } else {
            int i3 = this.mBarLength;
            if (i2 > i3) {
                i2 = i3;
            }
        }
        int iHSVToColor = Color.HSVToColor(Math.round(this.mPosToOpacFactor * i2), this.mHSVColor);
        this.mColor = iHSVToColor;
        if (Color.alpha(iHSVToColor) > 250) {
            this.mColor = Color.HSVToColor(this.mHSVColor);
        } else {
            Color.alpha(this.mColor);
        }
    }

    public int getColor() {
        return getMatchColor(this.mColor);
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
        bundle.putInt(STATE_OPACITY, getOpacity());
        return bundle;
    }

    @Override // android.view.View
    protected void onRestoreInstanceState(Parcelable parcelable) {
        Bundle bundle = (Bundle) parcelable;
        super.onRestoreInstanceState(bundle.getParcelable(STATE_PARENT));
        setColor(Color.HSVToColor(bundle.getFloatArray("color")));
        setOpacity(bundle.getInt(STATE_OPACITY));
    }
}