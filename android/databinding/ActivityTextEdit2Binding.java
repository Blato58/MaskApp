package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.RadioButton;
import android.widget.RelativeLayout;
import android.widget.SeekBar;
import android.widget.TextView;
import androidx.recyclerview.widget.RecyclerView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.widget.Circle;
import cn.com.heaton.shiningmask.ui.widget.LedTextView;
import cn.com.heaton.shiningmask.ui.widget.MultiLineRadioGroup;
import cn.com.heaton.shiningmask.ui.widget.holocolorpicker.ColorPicker;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityTextEdit2Binding implements ViewBinding {
    public final Circle circleColor;
    public final ColorPicker colorPicker1;
    public final ColorPicker colorPicker2;
    public final ColorPicker colorPicker3;
    public final EditText etTextInput;
    public final ImageButton ibtnSend;
    public final ImageView ivAminMode;
    public final ImageView ivCenterSelect;
    public final ImageView ivCenterSelect1;
    public final ImageView ivCenterSelect3;
    public final ImageView ivCenterSelectBg;
    public final ImageView ivCenterSelectBgBottom;
    public final ImageView ivCenterSelectBottom;
    public final ImageView ivGo;
    public final ImageView ivModeNext;
    public final ImageView ivModePrevious;
    public final ImageView ivOffLight;
    public final ImageView ivOffLight3;
    public final ImageView ivOffLightBg;
    public final ImageView ivOk;
    public final ImageView ivZhizhen1;
    public final ImageView ivZhizhen2;
    public final ImageView ivZhizhen3;
    public final LinearLayout llBottom;
    public final RelativeLayout llCenter;
    public final LinearLayout llColorBgUnselected;
    public final LinearLayout llColorTop;
    public final LinearLayout llColorTop1;
    public final LinearLayout llColorTop11;
    public final LinearLayout llColorUnselected;
    public final LinearLayout llLedViewPreview;
    public final LinearLayout llTextAdd;
    public final RelativeLayout llTextAddTop;
    public final RelativeLayout llTextColor;
    public final RelativeLayout llTextEdit;
    public final LinearLayout llTextEdit1;
    public final LinearLayout llTextPreview;
    public final RelativeLayout llTextSpeed;
    public final LedTextView ltvPreview;
    public final RadioButton rbGradientSelect1;
    public final RadioButton rbGradientSelect2;
    public final RadioButton rbGradientSelect3;
    public final RadioButton rbGradientSelect4;
    public final MultiLineRadioGroup rbTextSelect;
    public final RadioButton rbTextSelect1;
    public final RadioButton rbTextSelect2;
    public final RadioButton rbTextSelect3;
    public final RadioButton rbTextSelect4;
    public final MultiLineRadioGroup rgTextBgClolor;
    public final RelativeLayout rlAnim;
    public final RelativeLayout rlColor;
    public final RelativeLayout rlColor1;
    public final RelativeLayout rlColor3;
    public final RelativeLayout rlSpeedAnim;
    public final RelativeLayout rootView;
    private final RelativeLayout rootView_;
    public final ListView rvHistoryList;
    public final RecyclerView rvImageIconList;
    public final RecyclerView rvLedviewList;
    public final SeekBar sbMoveLight;
    public final LayoutTitlebar2Binding top;
    public final View viewLed;
    public final TextView viewTextPickcolor1;
    public final View viewTextPickcolor2;
    public final View viewTop;

    private ActivityTextEdit2Binding(RelativeLayout relativeLayout, Circle circle, ColorPicker colorPicker, ColorPicker colorPicker2, ColorPicker colorPicker3, EditText editText, ImageButton imageButton, ImageView imageView, ImageView imageView2, ImageView imageView3, ImageView imageView4, ImageView imageView5, ImageView imageView6, ImageView imageView7, ImageView imageView8, ImageView imageView9, ImageView imageView10, ImageView imageView11, ImageView imageView12, ImageView imageView13, ImageView imageView14, ImageView imageView15, ImageView imageView16, ImageView imageView17, LinearLayout linearLayout, RelativeLayout relativeLayout2, LinearLayout linearLayout2, LinearLayout linearLayout3, LinearLayout linearLayout4, LinearLayout linearLayout5, LinearLayout linearLayout6, LinearLayout linearLayout7, LinearLayout linearLayout8, RelativeLayout relativeLayout3, RelativeLayout relativeLayout4, RelativeLayout relativeLayout5, LinearLayout linearLayout9, LinearLayout linearLayout10, RelativeLayout relativeLayout6, LedTextView ledTextView, RadioButton radioButton, RadioButton radioButton2, RadioButton radioButton3, RadioButton radioButton4, MultiLineRadioGroup multiLineRadioGroup, RadioButton radioButton5, RadioButton radioButton6, RadioButton radioButton7, RadioButton radioButton8, MultiLineRadioGroup multiLineRadioGroup2, RelativeLayout relativeLayout7, RelativeLayout relativeLayout8, RelativeLayout relativeLayout9, RelativeLayout relativeLayout10, RelativeLayout relativeLayout11, RelativeLayout relativeLayout12, ListView listView, RecyclerView recyclerView, RecyclerView recyclerView2, SeekBar seekBar, LayoutTitlebar2Binding layoutTitlebar2Binding, View view, TextView textView, View view2, View view3) {
        this.rootView_ = relativeLayout;
        this.circleColor = circle;
        this.colorPicker1 = colorPicker;
        this.colorPicker2 = colorPicker2;
        this.colorPicker3 = colorPicker3;
        this.etTextInput = editText;
        this.ibtnSend = imageButton;
        this.ivAminMode = imageView;
        this.ivCenterSelect = imageView2;
        this.ivCenterSelect1 = imageView3;
        this.ivCenterSelect3 = imageView4;
        this.ivCenterSelectBg = imageView5;
        this.ivCenterSelectBgBottom = imageView6;
        this.ivCenterSelectBottom = imageView7;
        this.ivGo = imageView8;
        this.ivModeNext = imageView9;
        this.ivModePrevious = imageView10;
        this.ivOffLight = imageView11;
        this.ivOffLight3 = imageView12;
        this.ivOffLightBg = imageView13;
        this.ivOk = imageView14;
        this.ivZhizhen1 = imageView15;
        this.ivZhizhen2 = imageView16;
        this.ivZhizhen3 = imageView17;
        this.llBottom = linearLayout;
        this.llCenter = relativeLayout2;
        this.llColorBgUnselected = linearLayout2;
        this.llColorTop = linearLayout3;
        this.llColorTop1 = linearLayout4;
        this.llColorTop11 = linearLayout5;
        this.llColorUnselected = linearLayout6;
        this.llLedViewPreview = linearLayout7;
        this.llTextAdd = linearLayout8;
        this.llTextAddTop = relativeLayout3;
        this.llTextColor = relativeLayout4;
        this.llTextEdit = relativeLayout5;
        this.llTextEdit1 = linearLayout9;
        this.llTextPreview = linearLayout10;
        this.llTextSpeed = relativeLayout6;
        this.ltvPreview = ledTextView;
        this.rbGradientSelect1 = radioButton;
        this.rbGradientSelect2 = radioButton2;
        this.rbGradientSelect3 = radioButton3;
        this.rbGradientSelect4 = radioButton4;
        this.rbTextSelect = multiLineRadioGroup;
        this.rbTextSelect1 = radioButton5;
        this.rbTextSelect2 = radioButton6;
        this.rbTextSelect3 = radioButton7;
        this.rbTextSelect4 = radioButton8;
        this.rgTextBgClolor = multiLineRadioGroup2;
        this.rlAnim = relativeLayout7;
        this.rlColor = relativeLayout8;
        this.rlColor1 = relativeLayout9;
        this.rlColor3 = relativeLayout10;
        this.rlSpeedAnim = relativeLayout11;
        this.rootView = relativeLayout12;
        this.rvHistoryList = listView;
        this.rvImageIconList = recyclerView;
        this.rvLedviewList = recyclerView2;
        this.sbMoveLight = seekBar;
        this.top = layoutTitlebar2Binding;
        this.viewLed = view;
        this.viewTextPickcolor1 = textView;
        this.viewTextPickcolor2 = view2;
        this.viewTop = view3;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView_;
    }

    public static ActivityTextEdit2Binding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityTextEdit2Binding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_text_edit2, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityTextEdit2Binding bind(View view) {
        View viewFindChildViewById;
        View viewFindChildViewById2;
        View viewFindChildViewById3;
        int i = R.id.circle_color;
        Circle circle = (Circle) ViewBindings.findChildViewById(view, i);
        if (circle != null) {
            i = R.id.color_picker_1;
            ColorPicker colorPicker = (ColorPicker) ViewBindings.findChildViewById(view, i);
            if (colorPicker != null) {
                i = R.id.color_picker_2;
                ColorPicker colorPicker2 = (ColorPicker) ViewBindings.findChildViewById(view, i);
                if (colorPicker2 != null) {
                    i = R.id.color_picker_3;
                    ColorPicker colorPicker3 = (ColorPicker) ViewBindings.findChildViewById(view, i);
                    if (colorPicker3 != null) {
                        i = R.id.et_text_input;
                        EditText editText = (EditText) ViewBindings.findChildViewById(view, i);
                        if (editText != null) {
                            i = R.id.ibtn_send;
                            ImageButton imageButton = (ImageButton) ViewBindings.findChildViewById(view, i);
                            if (imageButton != null) {
                                i = R.id.iv_amin_mode;
                                ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
                                if (imageView != null) {
                                    i = R.id.iv_center_select;
                                    ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
                                    if (imageView2 != null) {
                                        i = R.id.iv_center_select_1;
                                        ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                                        if (imageView3 != null) {
                                            i = R.id.iv_center_select_3;
                                            ImageView imageView4 = (ImageView) ViewBindings.findChildViewById(view, i);
                                            if (imageView4 != null) {
                                                i = R.id.iv_center_select_bg;
                                                ImageView imageView5 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                if (imageView5 != null) {
                                                    i = R.id.iv_center_select_bg_bottom;
                                                    ImageView imageView6 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                    if (imageView6 != null) {
                                                        i = R.id.iv_center_select_bottom;
                                                        ImageView imageView7 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                        if (imageView7 != null) {
                                                            i = R.id.iv_go;
                                                            ImageView imageView8 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                            if (imageView8 != null) {
                                                                i = R.id.iv_mode_next;
                                                                ImageView imageView9 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                                if (imageView9 != null) {
                                                                    i = R.id.iv_mode_previous;
                                                                    ImageView imageView10 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                                    if (imageView10 != null) {
                                                                        i = R.id.iv_off_light;
                                                                        ImageView imageView11 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                                        if (imageView11 != null) {
                                                                            i = R.id.iv_off_light_3;
                                                                            ImageView imageView12 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                                            if (imageView12 != null) {
                                                                                i = R.id.iv_off_light_bg;
                                                                                ImageView imageView13 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                                                if (imageView13 != null) {
                                                                                    i = R.id.iv_ok;
                                                                                    ImageView imageView14 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                                                    if (imageView14 != null) {
                                                                                        i = R.id.iv_zhizhen1;
                                                                                        ImageView imageView15 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                                                        if (imageView15 != null) {
                                                                                            i = R.id.iv_zhizhen2;
                                                                                            ImageView imageView16 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                                                            if (imageView16 != null) {
                                                                                                i = R.id.iv_zhizhen3;
                                                                                                ImageView imageView17 = (ImageView) ViewBindings.findChildViewById(view, i);
                                                                                                if (imageView17 != null) {
                                                                                                    i = R.id.ll_bottom;
                                                                                                    LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                                                                    if (linearLayout != null) {
                                                                                                        i = R.id.ll_center;
                                                                                                        RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                        if (relativeLayout != null) {
                                                                                                            i = R.id.ll_color_bg_unselected;
                                                                                                            LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                                                                            if (linearLayout2 != null) {
                                                                                                                i = R.id.ll_color_top;
                                                                                                                LinearLayout linearLayout3 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                if (linearLayout3 != null) {
                                                                                                                    i = R.id.ll_color_top_1;
                                                                                                                    LinearLayout linearLayout4 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                    if (linearLayout4 != null) {
                                                                                                                        i = R.id.ll_color_top11;
                                                                                                                        LinearLayout linearLayout5 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                        if (linearLayout5 != null) {
                                                                                                                            i = R.id.ll_color_unselected;
                                                                                                                            LinearLayout linearLayout6 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                            if (linearLayout6 != null) {
                                                                                                                                i = R.id.ll_ledView_preview;
                                                                                                                                LinearLayout linearLayout7 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                if (linearLayout7 != null) {
                                                                                                                                    i = R.id.ll_text_add;
                                                                                                                                    LinearLayout linearLayout8 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                    if (linearLayout8 != null) {
                                                                                                                                        i = R.id.ll_text_add_top;
                                                                                                                                        RelativeLayout relativeLayout2 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                        if (relativeLayout2 != null) {
                                                                                                                                            i = R.id.ll_text_color;
                                                                                                                                            RelativeLayout relativeLayout3 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                            if (relativeLayout3 != null) {
                                                                                                                                                i = R.id.ll_text_edit;
                                                                                                                                                RelativeLayout relativeLayout4 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                                if (relativeLayout4 != null) {
                                                                                                                                                    i = R.id.ll_text_edit1;
                                                                                                                                                    LinearLayout linearLayout9 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                                    if (linearLayout9 != null) {
                                                                                                                                                        i = R.id.ll_text_preview;
                                                                                                                                                        LinearLayout linearLayout10 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                                        if (linearLayout10 != null) {
                                                                                                                                                            i = R.id.ll_text_speed;
                                                                                                                                                            RelativeLayout relativeLayout5 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                                            if (relativeLayout5 != null) {
                                                                                                                                                                i = R.id.ltv_preview;
                                                                                                                                                                LedTextView ledTextView = (LedTextView) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                if (ledTextView != null) {
                                                                                                                                                                    i = R.id.rb_gradient_select1;
                                                                                                                                                                    RadioButton radioButton = (RadioButton) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                    if (radioButton != null) {
                                                                                                                                                                        i = R.id.rb_gradient_select2;
                                                                                                                                                                        RadioButton radioButton2 = (RadioButton) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                        if (radioButton2 != null) {
                                                                                                                                                                            i = R.id.rb_gradient_select3;
                                                                                                                                                                            RadioButton radioButton3 = (RadioButton) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                            if (radioButton3 != null) {
                                                                                                                                                                                i = R.id.rb_gradient_select4;
                                                                                                                                                                                RadioButton radioButton4 = (RadioButton) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                if (radioButton4 != null) {
                                                                                                                                                                                    i = R.id.rb_text_select;
                                                                                                                                                                                    MultiLineRadioGroup multiLineRadioGroup = (MultiLineRadioGroup) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                    if (multiLineRadioGroup != null) {
                                                                                                                                                                                        i = R.id.rb_text_select1;
                                                                                                                                                                                        RadioButton radioButton5 = (RadioButton) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                        if (radioButton5 != null) {
                                                                                                                                                                                            i = R.id.rb_text_select2;
                                                                                                                                                                                            RadioButton radioButton6 = (RadioButton) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                            if (radioButton6 != null) {
                                                                                                                                                                                                i = R.id.rb_text_select3;
                                                                                                                                                                                                RadioButton radioButton7 = (RadioButton) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                if (radioButton7 != null) {
                                                                                                                                                                                                    i = R.id.rb_text_select4;
                                                                                                                                                                                                    RadioButton radioButton8 = (RadioButton) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                    if (radioButton8 != null) {
                                                                                                                                                                                                        i = R.id.rg_text_bg_clolor;
                                                                                                                                                                                                        MultiLineRadioGroup multiLineRadioGroup2 = (MultiLineRadioGroup) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                        if (multiLineRadioGroup2 != null) {
                                                                                                                                                                                                            i = R.id.rl_anim;
                                                                                                                                                                                                            RelativeLayout relativeLayout6 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                            if (relativeLayout6 != null) {
                                                                                                                                                                                                                i = R.id.rl_color;
                                                                                                                                                                                                                RelativeLayout relativeLayout7 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                                if (relativeLayout7 != null) {
                                                                                                                                                                                                                    i = R.id.rl_color_1;
                                                                                                                                                                                                                    RelativeLayout relativeLayout8 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                                    if (relativeLayout8 != null) {
                                                                                                                                                                                                                        i = R.id.rl_color_3;
                                                                                                                                                                                                                        RelativeLayout relativeLayout9 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                                        if (relativeLayout9 != null) {
                                                                                                                                                                                                                            i = R.id.rl_speed_anim;
                                                                                                                                                                                                                            RelativeLayout relativeLayout10 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                                            if (relativeLayout10 != null) {
                                                                                                                                                                                                                                RelativeLayout relativeLayout11 = (RelativeLayout) view;
                                                                                                                                                                                                                                i = R.id.rv_history_list;
                                                                                                                                                                                                                                ListView listView = (ListView) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                                                if (listView != null) {
                                                                                                                                                                                                                                    i = R.id.rv_image_icon_list;
                                                                                                                                                                                                                                    RecyclerView recyclerView = (RecyclerView) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                                                    if (recyclerView != null) {
                                                                                                                                                                                                                                        i = R.id.rv_ledview_list;
                                                                                                                                                                                                                                        RecyclerView recyclerView2 = (RecyclerView) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                                                        if (recyclerView2 != null) {
                                                                                                                                                                                                                                            i = R.id.sb_move_light;
                                                                                                                                                                                                                                            SeekBar seekBar = (SeekBar) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                                                            if (seekBar != null && (viewFindChildViewById = ViewBindings.findChildViewById(view, (i = R.id.top))) != null) {
                                                                                                                                                                                                                                                LayoutTitlebar2Binding layoutTitlebar2BindingBind = LayoutTitlebar2Binding.bind(viewFindChildViewById);
                                                                                                                                                                                                                                                i = R.id.view_led;
                                                                                                                                                                                                                                                View viewFindChildViewById4 = ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                                                                if (viewFindChildViewById4 != null) {
                                                                                                                                                                                                                                                    i = R.id.view_text_pickcolor1;
                                                                                                                                                                                                                                                    TextView textView = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                                                                                                                                                                                    if (textView != null && (viewFindChildViewById2 = ViewBindings.findChildViewById(view, (i = R.id.view_text_pickcolor2))) != null && (viewFindChildViewById3 = ViewBindings.findChildViewById(view, (i = R.id.view_top))) != null) {
                                                                                                                                                                                                                                                        return new ActivityTextEdit2Binding(relativeLayout11, circle, colorPicker, colorPicker2, colorPicker3, editText, imageButton, imageView, imageView2, imageView3, imageView4, imageView5, imageView6, imageView7, imageView8, imageView9, imageView10, imageView11, imageView12, imageView13, imageView14, imageView15, imageView16, imageView17, linearLayout, relativeLayout, linearLayout2, linearLayout3, linearLayout4, linearLayout5, linearLayout6, linearLayout7, linearLayout8, relativeLayout2, relativeLayout3, relativeLayout4, linearLayout9, linearLayout10, relativeLayout5, ledTextView, radioButton, radioButton2, radioButton3, radioButton4, multiLineRadioGroup, radioButton5, radioButton6, radioButton7, radioButton8, multiLineRadioGroup2, relativeLayout6, relativeLayout7, relativeLayout8, relativeLayout9, relativeLayout10, relativeLayout11, listView, recyclerView, recyclerView2, seekBar, layoutTitlebar2BindingBind, viewFindChildViewById4, textView, viewFindChildViewById2, viewFindChildViewById3);
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
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}