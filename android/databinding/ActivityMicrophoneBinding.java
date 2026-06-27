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
import cn.com.heaton.shiningmask.ui.widget.RhythmLedView;
import cn.com.heaton.shiningmask.ui.widget.loopviewpager.ViewPagerAllResponse;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityMicrophoneBinding implements ViewBinding {
    public final ImageView ivRhyImageBg2;
    public final ImageView ivRhybgTop;
    public final LinearLayout llTop2;
    public final RhythmLedView rhyledview1;
    public final RhythmLedView rhyledview2;
    public final RelativeLayout rlRhyBg;
    public final RelativeLayout rlRhySelect;
    public final RelativeLayout rlRhyShow;
    public final RelativeLayout rlRoot;
    private final RelativeLayout rootView;
    public final LayoutTitlebar1Binding top;
    public final ViewPagerAllResponse vpRhyhm;

    private ActivityMicrophoneBinding(RelativeLayout relativeLayout, ImageView imageView, ImageView imageView2, LinearLayout linearLayout, RhythmLedView rhythmLedView, RhythmLedView rhythmLedView2, RelativeLayout relativeLayout2, RelativeLayout relativeLayout3, RelativeLayout relativeLayout4, RelativeLayout relativeLayout5, LayoutTitlebar1Binding layoutTitlebar1Binding, ViewPagerAllResponse viewPagerAllResponse) {
        this.rootView = relativeLayout;
        this.ivRhyImageBg2 = imageView;
        this.ivRhybgTop = imageView2;
        this.llTop2 = linearLayout;
        this.rhyledview1 = rhythmLedView;
        this.rhyledview2 = rhythmLedView2;
        this.rlRhyBg = relativeLayout2;
        this.rlRhySelect = relativeLayout3;
        this.rlRhyShow = relativeLayout4;
        this.rlRoot = relativeLayout5;
        this.top = layoutTitlebar1Binding;
        this.vpRhyhm = viewPagerAllResponse;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityMicrophoneBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityMicrophoneBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_microphone, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityMicrophoneBinding bind(View view) {
        View viewFindChildViewById;
        int i = R.id.iv_rhy_image_bg2;
        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
        if (imageView != null) {
            i = R.id.iv_rhybg_top;
            ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView2 != null) {
                i = R.id.ll_top2;
                LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                if (linearLayout != null) {
                    i = R.id.rhyledview_1;
                    RhythmLedView rhythmLedView = (RhythmLedView) ViewBindings.findChildViewById(view, i);
                    if (rhythmLedView != null) {
                        i = R.id.rhyledview_2;
                        RhythmLedView rhythmLedView2 = (RhythmLedView) ViewBindings.findChildViewById(view, i);
                        if (rhythmLedView2 != null) {
                            i = R.id.rl_rhy_bg;
                            RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                            if (relativeLayout != null) {
                                i = R.id.rl_rhy_select;
                                RelativeLayout relativeLayout2 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                if (relativeLayout2 != null) {
                                    i = R.id.rl_rhy_show;
                                    RelativeLayout relativeLayout3 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                    if (relativeLayout3 != null) {
                                        i = R.id.rl_root;
                                        RelativeLayout relativeLayout4 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                        if (relativeLayout4 != null && (viewFindChildViewById = ViewBindings.findChildViewById(view, (i = R.id.top))) != null) {
                                            LayoutTitlebar1Binding layoutTitlebar1BindingBind = LayoutTitlebar1Binding.bind(viewFindChildViewById);
                                            i = R.id.vp_rhyhm;
                                            ViewPagerAllResponse viewPagerAllResponse = (ViewPagerAllResponse) ViewBindings.findChildViewById(view, i);
                                            if (viewPagerAllResponse != null) {
                                                return new ActivityMicrophoneBinding((RelativeLayout) view, imageView, imageView2, linearLayout, rhythmLedView, rhythmLedView2, relativeLayout, relativeLayout2, relativeLayout3, relativeLayout4, layoutTitlebar1BindingBind, viewPagerAllResponse);
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