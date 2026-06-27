package cn.com.heaton.shiningmask.ui.widget.colorpickerview;

import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Color;
import android.graphics.Matrix;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.utils.DensityUtil;

/* JADX INFO: loaded from: classes.dex */
public class ColorPickerView extends LinearLayout {
    private static final String TAG = "ColorPickerView";
    private Bitmap bitmapnew;
    private final RelativeLayout.LayoutParams colorBarLayoutParams;
    private int curImageRes;
    private final int heiget;
    private final View llColorProgress;
    private Context mContext;
    private OnColorChangeListener onColorChangeListener;
    private final View rlColorBar;
    private boolean touchEnable;
    private final View vColorBar;
    private final View viewColorBg;

    public ColorPickerView(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.touchEnable = true;
        this.mContext = context;
        View viewInflate = LayoutInflater.from(context).inflate(R.layout.view_color_picker, this);
        View viewFindViewById = findViewById(R.id.ll_color_progress);
        this.llColorProgress = viewFindViewById;
        this.viewColorBg = findViewById(R.id.view_color_bg);
        View viewFindViewById2 = viewInflate.findViewById(R.id.view_color_bar);
        this.vColorBar = viewFindViewById2;
        this.rlColorBar = viewInflate.findViewById(R.id.rl_color_bar);
        this.colorBarLayoutParams = (RelativeLayout.LayoutParams) viewFindViewById2.getLayoutParams();
        TypedArray typedArrayObtainStyledAttributes = context.obtainStyledAttributes(attributeSet, R.styleable.ColorPickerView);
        int i = typedArrayObtainStyledAttributes.getInt(R.styleable.ColorPickerView_color_wiew_height, 30);
        this.heiget = i;
        this.curImageRes = typedArrayObtainStyledAttributes.getResourceId(R.styleable.ColorPickerView_color_wiew_background, R.drawable.dd);
        typedArrayObtainStyledAttributes.recycle();
        setColorPickerBackgraoud(this.curImageRes);
        setColorPickerHeight(i);
        viewFindViewById.setOnTouchListener(new View.OnTouchListener() { // from class: cn.com.heaton.shiningmask.ui.widget.colorpickerview.ColorPickerView.1
            @Override // android.view.View.OnTouchListener
            public boolean onTouch(View view, MotionEvent motionEvent) {
                if (!ColorPickerView.this.touchEnable) {
                    return true;
                }
                ColorPickerView.this.setPreviewColor();
                return ColorPickerView.this.setLeftMargin((int) motionEvent.getX());
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean setLeftMargin(int i) {
        Bitmap bitmap;
        int width = this.llColorProgress.getWidth();
        int iDp2px = (int) DensityUtil.dp2px(this.mContext, 10.0f);
        int i2 = width - iDp2px;
        if (i <= 0) {
            i = 0;
        }
        this.colorBarLayoutParams.removeRule(11);
        if (i <= 0) {
            this.colorBarLayoutParams.leftMargin = i - iDp2px;
        } else if (i > i2) {
            int i3 = width - i;
            if (i3 < 0) {
                return true;
            }
            this.colorBarLayoutParams.addRule(11);
            this.colorBarLayoutParams.rightMargin = -(iDp2px - i3);
        } else {
            this.colorBarLayoutParams.leftMargin = i - iDp2px;
        }
        this.vColorBar.setLayoutParams(this.colorBarLayoutParams);
        if (i >= 0 && (bitmap = this.bitmapnew) != null) {
            onProgressChanged(i, iDp2px, width, bitmap);
        }
        return true;
    }

    public void setTouch(boolean z) {
        if (z) {
            this.touchEnable = true;
        } else {
            this.touchEnable = false;
        }
    }

    public void setColorProgress(float f) {
        Bitmap bitmap;
        int width = this.llColorProgress.getWidth();
        if (f == -1.0f) {
            f = width / 2;
        }
        int iDp2px = (int) DensityUtil.dp2px(this.mContext, 10.0f);
        this.colorBarLayoutParams.removeRule(11);
        if (f <= 0.0f) {
            this.colorBarLayoutParams.leftMargin = -iDp2px;
        } else if (f > width - iDp2px) {
            int i = (int) (width - f);
            if (i < 0) {
                return;
            }
            this.colorBarLayoutParams.addRule(11);
            this.colorBarLayoutParams.rightMargin = -(iDp2px - i);
        } else {
            this.colorBarLayoutParams.leftMargin = (int) (f - iDp2px);
        }
        this.vColorBar.setLayoutParams(this.colorBarLayoutParams);
        if (f >= 0.0f && (bitmap = this.bitmapnew) != null) {
            onProgressChanged(f, iDp2px, width, bitmap);
        }
        invalidate();
    }

    private void onProgressChanged(float f, float f2, int i, Bitmap bitmap) {
        if (bitmap == null || f >= i) {
            return;
        }
        try {
            int pixel = bitmap.getPixel((int) f, (int) f2);
            int iRed = Color.red(pixel);
            int iGreen = Color.green(pixel);
            int iBlue = Color.blue(pixel);
            OnColorChangeListener onColorChangeListener = this.onColorChangeListener;
            if (onColorChangeListener != null) {
                onColorChangeListener.colorChanged(pixel, iRed, iGreen, iBlue, f);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public static Bitmap imageScale(Bitmap bitmap, int i, int i2) {
        int width = bitmap.getWidth();
        int height = bitmap.getHeight();
        Matrix matrix = new Matrix();
        matrix.postScale(i / width, i2 / height);
        try {
            return Bitmap.createBitmap(bitmap, 0, 0, width, height, matrix, true);
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }

    @Override // android.view.View
    public void onWindowFocusChanged(boolean z) {
        super.onWindowFocusChanged(z);
    }

    public void setOnColorChangeListener(OnColorChangeListener onColorChangeListener) {
        this.onColorChangeListener = onColorChangeListener;
    }

    public void setColorPickerBackgraoud(int i) {
        this.viewColorBg.setBackgroundResource(i);
        this.curImageRes = i;
    }

    public void setColorPickerHeight(int i) {
        try {
            int iDp2px = (int) DensityUtil.dp2px(this.mContext, i);
            this.viewColorBg.setLayoutParams(new LinearLayout.LayoutParams(-1, iDp2px));
            this.rlColorBar.setLayoutParams(new LinearLayout.LayoutParams(-1, iDp2px));
            RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(iDp2px, iDp2px);
            layoutParams.setMarginStart((int) DensityUtil.dp2px(this.mContext, 10.0f));
            layoutParams.addRule(11);
            layoutParams.addRule(15);
        } catch (ClassCastException e) {
            e.printStackTrace();
        } catch (Exception e2) {
            e2.printStackTrace();
        }
    }

    public void setPreviewColor() {
        Bitmap bitmapDecodeResource;
        int width = this.llColorProgress.getWidth();
        if (this.curImageRes != 0) {
            bitmapDecodeResource = BitmapFactory.decodeResource(getResources(), this.curImageRes);
        } else {
            bitmapDecodeResource = BitmapFactory.decodeResource(getResources(), R.drawable.dd);
        }
        if (bitmapDecodeResource == null) {
            return;
        }
        Bitmap bitmapImageScale = imageScale(bitmapDecodeResource, width, 80);
        this.bitmapnew = bitmapImageScale;
        if (bitmapImageScale != null) {
            onProgressChanged(1.0f, bitmapImageScale.getHeight() / 2, width, this.bitmapnew);
        }
    }

    public void setProgressIconVisible(boolean z) {
        if (z) {
            this.vColorBar.setVisibility(0);
        } else {
            this.vColorBar.setVisibility(8);
        }
        invalidate();
    }
}