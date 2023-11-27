<h1>Personnel Schedule Optimization Console Application</h1>

<h2>Overview</h2>
<p>
    This Personnel Schedule Optimization Console Application is specifically designed to assist retail store managers in scheduling their personnel effectively. The application leverages complex criteria, including income, sales count, working hours, and personnel expenses, to optimize the work schedules, ensuring maximum efficiency and profitability.
</p>

<h2>Objective</h2>
<p>
    The primary goal of this application is to allocate personnel to shifts in a retail environment based on various performance metrics and constraints. It aims to maximize store productivity by considering factors such as sales performance, work hours, and cost efficiency.
</p>

<h2>Key Features</h2>
<ul>
    <li><strong>Performance-Based Scheduling:</strong> Personnel are scheduled based on a combination of their income generation, sales count (invoicecount), and overall expenses.</li>
    <li><strong>Optimized Shift Allocation:</strong> The application ensures that personnel are allocated shifts that do not exceed half of the store's operational hours, aligning with their potential working hours.</li>
    <li><strong>Cost-Efficient Planning:</strong> By comparing income and expense ratios, the application identifies the most cost-effective personnel for each shift.</li>
    <li><strong>Dynamic Scheduling:</strong> Considers individual personnel availability and store requirements for each day of the week.</li>
</ul>

<h2>Ideal Users</h2>
<p>
    This application is ideal for retail store managers or schedulers who need to optimize their personnel allocation based on multiple performance metrics and operational constraints.
</p>

<h2>How It Works</h2>
<ul>
    <li><strong>Data Input:</strong> Users input personnel data, including sales performance, working hours, and potential shifts.</li>
    <li><strong>Algorithm:</strong> The application processes this data, considering store operational hours, personnel cost, and performance metrics.</li>
    <li><strong>Schedule Generation:</strong> The scheduler then generates the optimal personnel schedule for each day, ensuring the best balance between performance and cost.</li>
</ul>

<h2>Example Scenario</h2>
<p>
    Consider a day where two employees, 'user1' and 'user2', are available for the same shift. 'user2' generates significantly higher income and sales count than 'user1', but also comes at a higher expense. The application will analyze these factors and decide which employee's scheduling would bring more value to the store considering the cost-benefit ratio.
</p>

<h2>Components</h2>
<ul>
    <li><strong>Personnel Data Input:</strong> A JSON format input that includes detailed personnel metrics.</li>
    <li><strong>Scheduling Algorithm:</strong> Core logic that processes input data and applies scheduling criteria.</li>
    <li><strong>Output Schedule:</strong> The optimized schedule for store personnel based on the algorithm's decision.</li>
</ul>

<h2>Setup and Usage</h2>
<ul>
    <li>Ensure that .NET Core SDK and necessary dependencies are installed.</li>
    <li>Input the personnel data in the specified JSON format.</li>
    <li>Run the application to generate the optimized schedule.</li>
    <li>Review the schedule output in the console for daily personnel allocation.</li>
</ul>

<h2>Contribution</h2>
<p>
    This project is open for contributions and improvements. Feel free to fork the repository and submit pull requests.
</p>
