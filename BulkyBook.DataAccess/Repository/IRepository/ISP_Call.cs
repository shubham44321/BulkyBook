using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface ISP_Call :IDisposable
    {
        T Single<T>(string procedureName, DynamicParameters param = null);//Execute scalar or first row first column

        void Execute(string procedureName, DynamicParameters param = null);//Execupe sp no output

        T OneRecord<T>(string procedureName, DynamicParameters param = null);//complete one row

        IEnumerable<T> List<T>(string procedureName, DynamicParameters param = null);//single table

        Tuple<IEnumerable<T1>,IEnumerable<T2>> List<T1,T2>(string procedureName, DynamicParameters param = null);//multiple tables
    }
}
