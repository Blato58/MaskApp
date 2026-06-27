package cn.com.heaton.shiningmask.dao;

import android.database.Cursor;
import android.database.sqlite.SQLiteStatement;
import cn.com.heaton.shiningmask.dao.bean.InputTextRecord;
import org.greenrobot.greendao.AbstractDao;
import org.greenrobot.greendao.Property;
import org.greenrobot.greendao.database.Database;
import org.greenrobot.greendao.database.DatabaseStatement;
import org.greenrobot.greendao.internal.DaoConfig;

/* JADX INFO: loaded from: classes.dex */
public class InputTextRecordDao extends AbstractDao<InputTextRecord, Long> {
    public static final String TABLENAME = "INPUT_TEXT_RECORD";

    public static class Properties {
        public static final Property Id = new Property(0, Long.class, "id", true, "_id");
        public static final Property TextContent = new Property(1, String.class, "textContent", false, "TEXT_CONTENT");
    }

    @Override // org.greenrobot.greendao.AbstractDao
    protected final boolean isEntityUpdateable() {
        return true;
    }

    public InputTextRecordDao(DaoConfig daoConfig) {
        super(daoConfig);
    }

    public InputTextRecordDao(DaoConfig daoConfig, DaoSession daoSession) {
        super(daoConfig, daoSession);
    }

    public static void createTable(Database database, boolean z) {
        database.execSQL("CREATE TABLE " + (z ? "IF NOT EXISTS " : "") + "\"INPUT_TEXT_RECORD\" (\"_id\" INTEGER PRIMARY KEY AUTOINCREMENT ,\"TEXT_CONTENT\" TEXT);");
    }

    public static void dropTable(Database database, boolean z) {
        database.execSQL("DROP TABLE " + (z ? "IF EXISTS " : "") + "\"INPUT_TEXT_RECORD\"");
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final void bindValues(DatabaseStatement databaseStatement, InputTextRecord inputTextRecord) {
        databaseStatement.clearBindings();
        Long id = inputTextRecord.getId();
        if (id != null) {
            databaseStatement.bindLong(1, id.longValue());
        }
        String textContent = inputTextRecord.getTextContent();
        if (textContent != null) {
            databaseStatement.bindString(2, textContent);
        }
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final void bindValues(SQLiteStatement sQLiteStatement, InputTextRecord inputTextRecord) {
        sQLiteStatement.clearBindings();
        Long id = inputTextRecord.getId();
        if (id != null) {
            sQLiteStatement.bindLong(1, id.longValue());
        }
        String textContent = inputTextRecord.getTextContent();
        if (textContent != null) {
            sQLiteStatement.bindString(2, textContent);
        }
    }

    /* JADX WARN: Can't rename method to resolve collision */
    @Override // org.greenrobot.greendao.AbstractDao
    public Long readKey(Cursor cursor, int i) {
        if (cursor.isNull(i)) {
            return null;
        }
        return Long.valueOf(cursor.getLong(i));
    }

    /* JADX WARN: Can't rename method to resolve collision */
    @Override // org.greenrobot.greendao.AbstractDao
    public InputTextRecord readEntity(Cursor cursor, int i) {
        Long lValueOf = cursor.isNull(i) ? null : Long.valueOf(cursor.getLong(i));
        int i2 = i + 1;
        return new InputTextRecord(lValueOf, cursor.isNull(i2) ? null : cursor.getString(i2));
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public void readEntity(Cursor cursor, InputTextRecord inputTextRecord, int i) {
        inputTextRecord.setId(cursor.isNull(i) ? null : Long.valueOf(cursor.getLong(i)));
        int i2 = i + 1;
        inputTextRecord.setTextContent(cursor.isNull(i2) ? null : cursor.getString(i2));
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final Long updateKeyAfterInsert(InputTextRecord inputTextRecord, long j) {
        inputTextRecord.setId(Long.valueOf(j));
        return Long.valueOf(j);
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public Long getKey(InputTextRecord inputTextRecord) {
        if (inputTextRecord != null) {
            return inputTextRecord.getId();
        }
        return null;
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public boolean hasKey(InputTextRecord inputTextRecord) {
        return inputTextRecord.getId() != null;
    }
}