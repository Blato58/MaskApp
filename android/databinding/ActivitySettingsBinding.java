package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.CheckBox;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.RelativeLayout;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public final class ActivitySettingsBinding implements ViewBinding {
    public final ActivitySettingBinding aset;
    public final CheckBox cbGesture;
    public final LinearLayout checkUp;
    public final ImageView checkUpImage;
    public final TextView checkUpText;
    public final LinearLayout llFinger;
    public final LinearLayout llLanuages;
    public final LinearLayout llSettings;
    public final RadioButton rbAll;
    public final RadioButton rbDiy;
    public final RadioButton rbFinger;
    public final RadioButton rbImage;
    public final RadioButton rbLanguages;
    public final RadioButton rbOrder;
    public final RadioButton rbRandom;
    public final RadioGroup rgGallerySelection;
    public final RadioGroup rgLoop;
    public final RadioGroup rgMenu;
    private final RelativeLayout rootView;
    public final LayoutTitlebar1Binding top;
    public final TextView tvVersionSettings;

    private ActivitySettingsBinding(RelativeLayout relativeLayout, ActivitySettingBinding activitySettingBinding, CheckBox checkBox, LinearLayout linearLayout, ImageView imageView, TextView textView, LinearLayout linearLayout2, LinearLayout linearLayout3, LinearLayout linearLayout4, RadioButton radioButton, RadioButton radioButton2, RadioButton radioButton3, RadioButton radioButton4, RadioButton radioButton5, RadioButton radioButton6, RadioButton radioButton7, RadioGroup radioGroup, RadioGroup radioGroup2, RadioGroup radioGroup3, LayoutTitlebar1Binding layoutTitlebar1Binding, TextView textView2) {
        this.rootView = relativeLayout;
        this.aset = activitySettingBinding;
        this.cbGesture = checkBox;
        this.checkUp = linearLayout;
        this.checkUpImage = imageView;
        this.checkUpText = textView;
        this.llFinger = linearLayout2;
        this.llLanuages = linearLayout3;
        this.llSettings = linearLayout4;
        this.rbAll = radioButton;
        this.rbDiy = radioButton2;
        this.rbFinger = radioButton3;
        this.rbImage = radioButton4;
        this.rbLanguages = radioButton5;
        this.rbOrder = radioButton6;
        this.rbRandom = radioButton7;
        this.rgGallerySelection = radioGroup;
        this.rgLoop = radioGroup2;
        this.rgMenu = radioGroup3;
        this.top = layoutTitlebar1Binding;
        this.tvVersionSettings = textView2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivitySettingsBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivitySettingsBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_settings, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivitySettingsBinding bind(View view) {
        View viewFindChildViewById;
        int i = R.id.aset;
        View viewFindChildViewById2 = ViewBindings.findChildViewById(view, i);
        if (viewFindChildViewById2 != null) {
            ActivitySettingBinding activitySettingBindingBind = ActivitySettingBinding.bind(viewFindChildViewById2);
            i = R.id.cb_gesture;
            CheckBox checkBox = (CheckBox) ViewBindings.findChildViewById(view, i);
            if (checkBox != null) {
                i = R.id.check_up;
                LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                if (linearLayout != null) {
                    i = R.id.check_up_image;
                    ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
                    if (imageView != null) {
                        i = R.id.check_up_text;
                        TextView textView = (TextView) ViewBindings.findChildViewById(view, i);
                        if (textView != null) {
                            i = R.id.ll_finger;
                            LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                            if (linearLayout2 != null) {
                                i = R.id.ll_lanuages;
                                LinearLayout linearLayout3 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                if (linearLayout3 != null) {
                                    i = R.id.ll_settings;
                                    LinearLayout linearLayout4 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                    if (linearLayout4 != null) {
                                        i = R.id.rb_all;
                                        RadioButton radioButton = (RadioButton) ViewBindings.findChildViewById(view, i);
                                        if (radioButton != null) {
                                            i = R.id.rb_diy;
                                            RadioButton radioButton2 = (RadioButton) ViewBindings.findChildViewById(view, i);
                                            if (radioButton2 != null) {
                                                i = R.id.rb_finger;
                                                RadioButton radioButton3 = (RadioButton) ViewBindings.findChildViewById(view, i);
                                                if (radioButton3 != null) {
                                                    i = R.id.rb_image;
                                                    RadioButton radioButton4 = (RadioButton) ViewBindings.findChildViewById(view, i);
                                                    if (radioButton4 != null) {
                                                        i = R.id.rb_languages;
                                                        RadioButton radioButton5 = (RadioButton) ViewBindings.findChildViewById(view, i);
                                                        if (radioButton5 != null) {
                                                            i = R.id.rb_order;
                                                            RadioButton radioButton6 = (RadioButton) ViewBindings.findChildViewById(view, i);
                                                            if (radioButton6 != null) {
                                                                i = R.id.rb_random;
                                                                RadioButton radioButton7 = (RadioButton) ViewBindings.findChildViewById(view, i);
                                                                if (radioButton7 != null) {
                                                                    i = R.id.rg_gallery_selection;
                                                                    RadioGroup radioGroup = (RadioGroup) ViewBindings.findChildViewById(view, i);
                                                                    if (radioGroup != null) {
                                                                        i = R.id.rg_loop;
                                                                        RadioGroup radioGroup2 = (RadioGroup) ViewBindings.findChildViewById(view, i);
                                                                        if (radioGroup2 != null) {
                                                                            i = R.id.rg_menu;
                                                                            RadioGroup radioGroup3 = (RadioGroup) ViewBindings.findChildViewById(view, i);
                                                                            if (radioGroup3 != null && (viewFindChildViewById = ViewBindings.findChildViewById(view, (i = R.id.top))) != null) {
                                                                                LayoutTitlebar1Binding layoutTitlebar1BindingBind = LayoutTitlebar1Binding.bind(viewFindChildViewById);
                                                                                i = R.id.tv_version_settings;
                                                                                TextView textView2 = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                if (textView2 != null) {
                                                                                    return new ActivitySettingsBinding((RelativeLayout) view, activitySettingBindingBind, checkBox, linearLayout, imageView, textView, linearLayout2, linearLayout3, linearLayout4, radioButton, radioButton2, radioButton3, radioButton4, radioButton5, radioButton6, radioButton7, radioGroup, radioGroup2, radioGroup3, layoutTitlebar1BindingBind, textView2);
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