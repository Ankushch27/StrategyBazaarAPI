class AssignLicense extends React.Component {

    state = {
        results: []
    }
    handleclick = () => {
        console.log('click')
    }
    componentDidMount() {

        $.ajax({
            url: '/Home/GetAllDetails',
            success: function (result) {
                CouponsView.setState({
                    results: JSON.parse(result)
                });
                $('#DetailsTable').DataTable({
                    "pageLength": 20,
                    "paging": true,
                    "lengthChange": false,
                    "searching": false,
                    "ordering": true,
                    "info": true,
                    "autoWidth": false,
                    "responsive": true,
                });

            }
        });

    }

    getTimeStamp = (date) => {
        return moment(date, 'DD-MM-yyyy HH:mm:ss').unix()
    }
    renderTableData() {
        return (
            this.state.results.length > 0 ?
                this.state.results.map((item, i) => {
                    return (
                        <tr key={i}>
                            <td><span>{this.getTimeStamp(item.Timestamp)}</span>{item.Timestamp}</td>
                            <td>{item.Username}</td>
                            <td>{item.Email}</td>
                            <td>{item.Mobile}</td>
                            <td>{(item.Module == "1" ? "Free" : item.Module == "2" ? "Intraday" : "Positional")}</td>
                            <td><span>{this.getTimeStamp(item.Expiry)}</span>{item.Expiry}</td>
                            <td>{item.LastAmountPaid}</td>
                            <td><span>{this.getTimeStamp(item.DateModified)}</span>{item.DateModified}</td>

                        </tr>
                    );
                })
                :
                <tr>
                    <td colSpan="7" className="text-center">No records found.</td>
                </tr>
        );
    }
    render() {
        return (
            <div className="content-wrapper pt-4">
                <section className="content-header">
                    <div className="container-fluid">
                        <div className="row mb-2">
                            <div className="col-sm-6">
                                <h1>User Details</h1>
                            </div>
                            <div className="col-sm-2 ml-auto">
                                <a className="btn btn-outline-primary btn-block" href="/Home/DownloadCSV">  Get CSV </a>
                            </div>
                        </div>
                    </div>
                </section>
                <section className="content">
                    <div className="container-fluid">
                        <div className="row">
                            <div className="col-12">
                                <div className="card">
                                    <div className="card-body">
                                        <table id="DetailsTable" className="table table-bordered table-hover">
                                            <thead>
                                                <tr>
                                                    <th>Timestamp</th>
                                                    <th>Username</th>
                                                    <th>Email</th>
                                                    <th>Mobile</th>
                                                    <th>Module</th>
                                                    <th>Expiry</th>
                                                    <th>LastAmountPaid</th>
                                                    <th>DateModified</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {this.renderTableData()}
                                            </tbody>
                                        </table>

                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </section>
            </div>
        )
    }
}

var CouponsView = ReactDOM.render(<AssignLicense />, document.getElementById('Homes'));