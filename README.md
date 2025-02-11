# Description:
Real life Online Store Management System Full-stack solution handling transactions, customers, orders, shipping,Product, Reviews, and delivery tracking.

# Project structure:
1. Twinkies Store project files: contains the icons used in UI, chatbot prompts helped in developing Data/Business layers.
2. TSDB.bak: backup of the used database, it contains the db diagram, business logic stored procedures.
3. Twinkies Store Data Access Layer: contains a csproj file containing the Database access layer classes for each entity/table in the database, uses T-sql stored procedures(found in TSDB.bak)thus following all business rules.
4. Twinkies Store Business Layer: contains a csproj file contianing all the business layer classes used to implement any kind of interface.
5. Test Presentation layer: ASP.NET Web APP test intrface for the Twinkies Store management app (not implemented).
   
# Technologies
1. MSSQL form database management, altering, and admininstration.
2. T-SQL for implementing all needed stored procedures and forcing business logic
3. C# used to build the Data acccess, Business layers, validation, caching.
4. .Net 9 framework used for Data access, Business layers class librairies.
5.  ADO.Net used to connect and query on the data base.
6.  GitHub used to save progress, colaboration, version control.
7.  Claude 3.5 Sonnet used to accelerate the process of implementing each stored procedure, Data access layer, business layer.

# Not Tested yet:
1. T-SQL Stored procedures. 
2. Data Access Layer.
3. Business Layer.

# Future steps:
1. Figure out the right framework for the web/mobile app.
2. Design and implement the UI App then perform manual tests on it.
3. Unit test each class inside Business/DataAccess layers.
4. Intergration testing betweeen each class.
5. System testing of all the implemented layers.
6. Performance Optimization for each DataAccess/Business class.
7. Scalabilty/Maintanability updates for each class.
8. Security/Validation updates for each class.
9. Perform a user acceptance test.
10. Adding custom validation -in addition to the already implemented validation- rules satisfying all business rules.
11. Implementing telemetry for each layer.


# -Huge thanks to DR.Amr abdelfatah for contributing to this project. 
