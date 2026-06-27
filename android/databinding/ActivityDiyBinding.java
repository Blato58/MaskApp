package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.widget.LedViewDiy;
import cn.com.heaton.shiningmask.ui.widget.RectView;
import cn.com.heaton.shiningmask.ui.widget.colorpickerview.ColorPickerView;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityDiyBinding implements ViewBinding {
    public final ColorPickerView cpvColorPreview;
    public final ImageView imgvClear;
    public final ImageView imgvDelete;
    public final ImageView imgvPaint;
    public final ImageView ivPen;
    public final ImageView ivPen2;
    public final ImageView ivPen3;
    public final LinearLayout llDiy;
    public final RelativeLayout lnrlaoBottomEdit;
    public final LedViewDiy lvEdit;
    public final RelativeLayout rlTop;
    private final RelativeLayout rootView;
    public final RectView rvColor;
    public final LayoutTitlebar1Binding top;

    private ActivityDiyBinding(RelativeLayout relativeLayout, ColorPickerView colorPickerView, ImageView imageView, ImageView imageView2, ImageView imageView3, ImageView imageView4, ImageView imageView5, ImageView imageView6, LinearLayout linearLayout, RelativeLayout relativeLayout2, LedViewDiy ledViewDiy, RelativeLayout relativeLayout3, RectView rectView, LayoutTitlebar1Binding layoutTitlebar1Binding) {
        this.rootView = relativeLayout;
        this.cpvColorPreview = colorPickerView;
        this.imgvClear = imageView;
        this.imgvDelete = imageView2;
        this.imgvPaint = imageView3;
        this.ivPen = imageView4;
        this.ivPen2 = imageView5;
        this.ivPen3 = imageView6;
        this.llDiy = linearLayout;
        this.lnrlaoBottomEdit = relativeLayout2;
        this.lvEdit = ledViewDiy;
        this.rlTop = relativeLayout3;
        this.rvColor = rectView;
        this.top = layoutTitlebar1Binding;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityDiyBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityDiyBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_diy, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityDiyBinding bind(View view) {
        View viewFindChildViewById;
        int i = R.id.cpv_color_preview;
        ColorPickerView colorPickerView = (ColorPickerView) ViewBindings.findChildViewById(view, i);
        if (colorPickerView != null) {
            i = R.id.imgv_clear;
            ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView != null) {
                i = R.id.imgv_delete;
                ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
                if (imageView2 != null) {
                    i = R.id.imgv_paint;
                    ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                    if (imageView3 != null) {
                        i = R.id.iv_pen;
                        ImageView imageView4 = (ImageView) ViewBindings.findChildViewById(view, i);
                        if (imageView4 != null) {
                            i = R.id.iv_pen2;
                            ImageView imageView5 = (ImageView) ViewBindings.findChildViewById(view, i);
                            if (imageView5 != null) {
                                i = R.id.iv_pen3;
                                ImageView imageView6 = (ImageView) ViewBindings.findChildViewById(view, i);
                                if (imageView6 != null) {
                                    i = R.id.ll_diy;
                                    LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                    if (linearLayout != null) {
                                        i = R.id.lnrlao_bottom_edit;
                                        RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                        if (relativeLayout != null) {
                                            i = R.id.lv_edit;
                                            LedViewDiy ledViewDiy = (LedViewDiy) ViewBindings.findChildViewById(view, i);
                                            if (ledViewDiy != null) {
                                                i = R.id.rl_top;
                                                RelativeLayout relativeLayout2 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                if (relativeLayout2 != null) {
                                                    i = R.id.rv_color;
                                                    RectView rectView = (RectView) ViewBindings.findChildViewById(view, i);
                                                    if (rectView != null && (viewFindChildViewById = ViewBindings.findChildViewById(view, (i = R.id.top))) != null) {
                                                        return new ActivityDiyBinding((RelativeLayout) view, colorPickerView, imageView, imageView2, imageView3, imageView4, imageView5, imageView6, linearLayout, relativeLayout, ledViewDiy, relativeLayout2, rectView, LayoutTitlebar1Binding.bind(viewFindChildViewById));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}