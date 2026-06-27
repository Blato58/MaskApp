package cn.com.heaton.shiningmask.dao;

import java.util.Arrays;
import java.util.Iterator;
import java.util.List;
import org.greenrobot.greendao.converter.PropertyConverter;

/* JADX INFO: loaded from: classes.dex */
public class StringConverter implements PropertyConverter<List<String>, String> {
    @Override // org.greenrobot.greendao.converter.PropertyConverter
    public List<String> convertToEntityProperty(String str) {
        if (str == null) {
            return null;
        }
        return Arrays.asList(str.split(","));
    }

    @Override // org.greenrobot.greendao.converter.PropertyConverter
    public String convertToDatabaseValue(List<String> list) {
        if (list == null) {
            return null;
        }
        StringBuilder sb = new StringBuilder();
        Iterator<String> it = list.iterator();
        while (it.hasNext()) {
            sb.append(it.next());
            sb.append(",");
        }
        return sb.toString();
    }
}