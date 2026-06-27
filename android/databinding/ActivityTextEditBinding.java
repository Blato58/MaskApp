package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.RelativeLayout;
import android.widget.SeekBar;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityTextEditBinding implements ViewBinding {
    public final EditText etTextInput;
    public final ImageView ivText1;
    public final ImageView ivText2;
    public final ImageView ivText3;
    public final LinearLayout llContent;
    public final LinearLayout llLedView1;
    public final LinearLayout llLedView2;
    public final LinearLayout llLedView3;
    public final LinearLayout llSeekbar;
    public final LinearLayout llSpeed;
    public final RelativeLayout llTop1;
    public final ListView lvRecordList;
    public final LinearLayout rlBottom;
    public final RelativeLayout rlRoot;
    private final RelativeLayout rootView;
    public final SeekBar sbMoveLight;
    public final LayoutTitlebar1Binding top;
    public final TextView tvGo;
    public final TextView tvInputShow;

    private ActivityTextEditBinding(RelativeLayout relativeLayout, EditText editText, ImageView imageView, ImageView imageView2, ImageView imageView3, LinearLayout linearLayout, LinearLayout linearLayout2, LinearLayout linearLayout3, LinearLayout linearLayout4, LinearLayout linearLayout5, LinearLayout linearLayout6, RelativeLayout relativeLayout2, ListView listView, LinearLayout linearLayout7, RelativeLayout relativeLayout3, SeekBar seekBar, LayoutTitlebar1Binding layoutTitlebar1Binding, TextView textView, TextView textView2) {
        this.rootView = relativeLayout;
        this.etTextInput = editText;
        this.ivText1 = imageView;
        this.ivText2 = imageView2;
        this.ivText3 = imageView3;
        this.llContent = linearLayout;
        this.llLedView1 = linearLayout2;
        this.llLedView2 = linearLayout3;
        this.llLedView3 = linearLayout4;
        this.llSeekbar = linearLayout5;
        this.llSpeed = linearLayout6;
        this.llTop1 = relativeLayout2;
        this.lvRecordList = listView;
        this.rlBottom = linearLayout7;
        this.rlRoot = relativeLayout3;
        this.sbMoveLight = seekBar;
        this.top = layoutTitlebar1Binding;
        this.tvGo = textView;
        this.tvInputShow = textView2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityTextEditBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityTextEditBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_text_edit, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityTextEditBinding bind(View view) {
        View viewFindChildViewById;
        int i = R.id.et_text_input;
        EditText editText = (EditText) ViewBindings.findChildViewById(view, i);
        if (editText != null) {
            i = R.id.iv_text1;
            ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView != null) {
                i = R.id.iv_text2;
                ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
                if (imageView2 != null) {
                    i = R.id.iv_text3;
                    ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                    if (imageView3 != null) {
                        i = R.id.ll_content;
                        LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                        if (linearLayout != null) {
                            i = R.id.ll_ledView1;
                            LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                            if (linearLayout2 != null) {
                                i = R.id.ll_ledView2;
                                LinearLayout linearLayout3 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                if (linearLayout3 != null) {
                                    i = R.id.ll_ledView3;
                                    LinearLayout linearLayout4 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                    if (linearLayout4 != null) {
                                        i = R.id.ll_seekbar;
                                        LinearLayout linearLayout5 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                        if (linearLayout5 != null) {
                                            i = R.id.ll_speed;
                                            LinearLayout linearLayout6 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                            if (linearLayout6 != null) {
                                                i = R.id.ll_top1;
                                                RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                if (relativeLayout != null) {
                                                    i = R.id.lv_record_list;
                                                    ListView listView = (ListView) ViewBindings.findChildViewById(view, i);
                                                    if (listView != null) {
                                                        i = R.id.rl_bottom;
                                                        LinearLayout linearLayout7 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                        if (linearLayout7 != null) {
                                                            RelativeLayout relativeLayout2 = (RelativeLayout) view;
                                                            i = R.id.sb_move_light;
                                                            SeekBar seekBar = (SeekBar) ViewBindings.findChildViewById(view, i);
                                                            if (seekBar != null && (viewFindChildViewById = ViewBindings.findChildViewById(view, (i = R.id.top))) != null) {
                                                                LayoutTitlebar1Binding layoutTitlebar1BindingBind = LayoutTitlebar1Binding.bind(viewFindChildViewById);
                                                                i = R.id.tv_go;
                                                                TextView textView = (TextView) ViewBindings.findChildViewById(view, i);
                                                                if (textView != null) {
                                                                    i = R.id.tv_input_show;
                                                                    TextView textView2 = (TextView) ViewBindings.findChildViewById(view, i);
                                                                    if (textView2 != null) {
                                                                        return new ActivityTextEditBinding(relativeLayout2, editText, imageView, imageView2, imageView3, linearLayout, linearLayout2, linearLayout3, linearLayout4, linearLayout5, linearLayout6, relativeLayout, listView, linearLayout7, relativeLayout2, seekBar, layoutTitlebar1BindingBind, textView, textView2);
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