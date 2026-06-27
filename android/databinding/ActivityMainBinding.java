package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;
import android.widget.SeekBar;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityMainBinding implements ViewBinding {
    public final ImageView ivAnimDiy;
    public final ImageView ivAnimFft;
    public final ImageView ivAnimImage;
    public final ImageView ivAnimText;
    public final LinearLayout llDiy;
    public final LinearLayout llImage;
    public final LinearLayout llRhythm;
    public final LinearLayout llSeekbar;
    public final LinearLayout llText;
    public final LinearLayout rlTop;
    private final RelativeLayout rootView;
    public final SeekBar sbMoveLight;
    public final LayoutTitlebarBinding top;

    private ActivityMainBinding(RelativeLayout relativeLayout, ImageView imageView, ImageView imageView2, ImageView imageView3, ImageView imageView4, LinearLayout linearLayout, LinearLayout linearLayout2, LinearLayout linearLayout3, LinearLayout linearLayout4, LinearLayout linearLayout5, LinearLayout linearLayout6, SeekBar seekBar, LayoutTitlebarBinding layoutTitlebarBinding) {
        this.rootView = relativeLayout;
        this.ivAnimDiy = imageView;
        this.ivAnimFft = imageView2;
        this.ivAnimImage = imageView3;
        this.ivAnimText = imageView4;
        this.llDiy = linearLayout;
        this.llImage = linearLayout2;
        this.llRhythm = linearLayout3;
        this.llSeekbar = linearLayout4;
        this.llText = linearLayout5;
        this.rlTop = linearLayout6;
        this.sbMoveLight = seekBar;
        this.top = layoutTitlebarBinding;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityMainBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityMainBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_main, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityMainBinding bind(View view) {
        View viewFindChildViewById;
        int i = R.id.iv_anim_diy;
        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
        if (imageView != null) {
            i = R.id.iv_anim_fft;
            ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView2 != null) {
                i = R.id.iv_anim_image;
                ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                if (imageView3 != null) {
                    i = R.id.iv_anim_text;
                    ImageView imageView4 = (ImageView) ViewBindings.findChildViewById(view, i);
                    if (imageView4 != null) {
                        i = R.id.ll_diy;
                        LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                        if (linearLayout != null) {
                            i = R.id.ll_image;
                            LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                            if (linearLayout2 != null) {
                                i = R.id.ll_rhythm;
                                LinearLayout linearLayout3 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                if (linearLayout3 != null) {
                                    i = R.id.ll_seekbar;
                                    LinearLayout linearLayout4 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                    if (linearLayout4 != null) {
                                        i = R.id.ll_text;
                                        LinearLayout linearLayout5 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                        if (linearLayout5 != null) {
                                            i = R.id.rl_top;
                                            LinearLayout linearLayout6 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                            if (linearLayout6 != null) {
                                                i = R.id.sb_move_light;
                                                SeekBar seekBar = (SeekBar) ViewBindings.findChildViewById(view, i);
                                                if (seekBar != null && (viewFindChildViewById = ViewBindings.findChildViewById(view, (i = R.id.top))) != null) {
                                                    return new ActivityMainBinding((RelativeLayout) view, imageView, imageView2, imageView3, imageView4, linearLayout, linearLayout2, linearLayout3, linearLayout4, linearLayout5, linearLayout6, seekBar, LayoutTitlebarBinding.bind(viewFindChildViewById));
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