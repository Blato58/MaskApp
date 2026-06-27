package cn.com.heaton.shiningmask.ui.widget;

import android.content.Context;
import android.text.Editable;
import android.text.TextUtils;
import android.text.TextWatcher;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.Button;
import android.widget.EditText;
import android.widget.PopupWindow;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;

/* JADX INFO: loaded from: classes.dex */
public class EditPopWindow extends PopupWindow {
    private Button btnRight;
    private EditChangedLisenter editChangedLisenter;
    private EditText et_board;
    private Context mContext;

    public interface EditChangedLisenter {
        void afterTextChanged(String str);

        void onClickOk(String str);
    }

    public void setMaxLenth(int i) {
    }

    public EditPopWindow(Context context) {
        super(context);
        this.mContext = context;
        initPopupWindow();
        View viewInflate = View.inflate(context, R.layout.view_edit, null);
        setContentView(viewInflate);
        setWidth(-1);
        setHeight(-2);
        initView(viewInflate);
    }

    public void setEditChangedLisenter(EditChangedLisenter editChangedLisenter) {
        this.editChangedLisenter = editChangedLisenter;
    }

    private void initView(View view) {
        EditText editText = (EditText) view.findViewById(R.id.et_board);
        this.et_board = editText;
        editText.addTextChangedListener(new TextWatcherImpl());
        Button button = (Button) view.findViewById(R.id.btn_right);
        this.btnRight = button;
        button.setOnClickListener(new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.widget.EditPopWindow.1
            @Override // android.view.View.OnClickListener
            public void onClick(View view2) {
                if (EditPopWindow.this.editChangedLisenter == null || EditPopWindow.this.et_board.getText().toString() == null) {
                    return;
                }
                EditPopWindow.this.editChangedLisenter.onClickOk(EditPopWindow.this.et_board.getText().toString());
                EditPopWindow.this.et_board.setText("");
            }
        });
    }

    public void setEidtText(String str) {
        if (TextUtils.isEmpty(str)) {
            return;
        }
        this.et_board.setText(str);
        this.et_board.setSelection(str.length());
    }

    private void initPopupWindow() {
        setInputMethodMode(1);
        setSoftInputMode(16);
        setFocusable(true);
        setOutsideTouchable(true);
    }

    private class TextWatcherImpl implements TextWatcher {
        @Override // android.text.TextWatcher
        public void beforeTextChanged(CharSequence charSequence, int i, int i2, int i3) {
        }

        @Override // android.text.TextWatcher
        public void onTextChanged(CharSequence charSequence, int i, int i2, int i3) {
        }

        private TextWatcherImpl() {
        }

        @Override // android.text.TextWatcher
        public void afterTextChanged(Editable editable) {
            if (TextUtils.isEmpty(editable) || EditPopWindow.this.editChangedLisenter == null) {
                return;
            }
            EditPopWindow.this.editChangedLisenter.afterTextChanged(editable.toString());
        }
    }

    public void showPopupWindow(View view) {
        if (!isShowing()) {
            LogUtil.d("以下拉方式显示popupwindow");
            this.et_board.requestFocus();
            ((InputMethodManager) this.mContext.getSystemService("input_method")).toggleSoftInput(0, 2);
            showAtLocation(view, 81, 0, 0);
            return;
        }
        dismiss();
    }

    public void hideSoftInput() {
        LogUtil.d("hideSoftInput");
        this.et_board.clearFocus();
        ((InputMethodManager) this.mContext.getSystemService("input_method")).hideSoftInputFromWindow(this.et_board.getWindowToken(), 0);
    }
}