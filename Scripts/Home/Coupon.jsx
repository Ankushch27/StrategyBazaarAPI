class EditCoupon extends React.Component {

    state = {
        name: "",
        id: 0,
        percent: 100,
        active: true,
        License: "",
        mode: 0
    }


    componentDidMount() {
        $('#editProject').on('show.bs.modal', function (e) {
            AssignCoupon.setState({
                name: "",
                id: 0,
                percent: 99,
                active: true,
                License: "",
                mode: 0
            });
            var obj = $(event.target).data('obj');
            var mode = $(event.target).data('mode');
            if (mode == 1) {
                AssignCoupon.setState({
                    name: obj.name,
                    id: obj.id,
                    percent: obj.percent,
                    active: obj.active == 1 ? true : false,
                    License: obj.module.split(',')[0],
                    mode: mode
                });
            } else {
                AssignCoupon.setState({
                    mode: mode
                });
            }
            //console.log(AssignCoupon.state.results)
        });
    }
    resetValues() {
        console.log("yes");

    }
    handleSubmitUser(event) {

        var flag = true;
        console.log(AssignCoupon.state.active);
        //validation
        if (AssignCoupon.state.License == '') {
            $('#Lisense').addClass('is-invalid');
            flag = false;
        }
        else {
            $('#Lisense').removeClass('is-invalid');
        }
        if (AssignCoupon.state.percent < 0) {
            $('#Percent').addClass('is-invalid');
            flag = false;
        }
        else {
            $('#Percent').removeClass('is-invalid');
        }

        if (!flag) {
            return;
        }

        $.ajax({
            data: {
                name: AssignCoupon.state.name.toUpperCase(),
                percent: AssignCoupon.state.percent,
                active: AssignCoupon.state.active,
                License: AssignCoupon.state.License,
                id: AssignCoupon.state.id
            },
            type: "POST",
            url: '/Home/ModifyCoupon',
            success: function (result) {
                $('#editProject').modal('hide');
                CouponsView.componentDidMount();
                AssignCoupon.setState({
                    name: "",
                    id: 0,
                    percent: 100,
                    active: true,
                    License: "",
                    mode: 0
                });
            },
            fail: function (result) {
                console.log(result);
            }
        });
        event.preventDefault();
    }

    render() {
        return (
            <div id="editProject" className="modal fade" tabIndex="-1" role="dialog">
                <div className="modal-dialog modal-lg">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h4 className="modal-title">Edit User</h4>
                            <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">×</span>
                            </button>
                        </div>
                        <div className="modal-body">
                            <form onSubmit={this.handleSubmit}>
                                <fieldset disabled={this.state.formState} className={(!this.state.loading ? ' show-element' : ' hide-element')}>
                                    <div className="form-group">
                                        <input value={this.state.name} disabled={this.state.mode == 1} onChange={(e) => { (this.state.mode == 1) ? console.log("Disabled") : AssignCoupon.setState({ name: e.target.value }) }} className="form-control col-sm-9 form-control-sm float-right" id="Name" type="text" />
                                        <span>Coupon Name</span>
                                    </div>

                                    <div className="form-group">
                                        <input value={100 - this.state.percent} onChange={(e) => { AssignCoupon.setState({ percent: 100 - e.target.value }); }} className="form-control col-sm-9 form-control-sm float-right" id="Percent" type="number" min="1" max="100" />
                                        <span>Discount %</span>
                                    </div>

                                    <div className="form-group">
                                        <select className="form-control col-sm-9 form-control-sm float-right" value={this.state.License} onChange={(e) => { AssignCoupon.setState({ License: e.target.value }); }} id="Lisense">
                                            <option value="">Choose Module</option>
                                            <option key="1" value="2">Intraday</option>
                                            <option key="2" value="3">Positional</option>
                                        </select>
                                        <span>API</span>
                                    </div>
                                    <div className="form-group row">
                                        <div className="col-sm-3">Active</div>
                                        <div className="col-sm-9">
                                            <div className="form-check">
                                                <input defaultChecked={this.state.active} id="1" className="form-check-input" type="checkbox" onChange={(e) => { AssignCoupon.setState({ active: e.target.value }); }} />
                                            </div></div></div>
                                </fieldset>
                            </form>
                        </div>
                        <div className="modal-footer justify-content-between">
                            <button type="button" onClick={this.handleSubmitUser} className="btn btn-primary">Save changes</button>
                        </div>
                    </div>
                </div>
            </div>
        )
    }
}

class Coupons extends React.Component {

    state = {
        results: []
    }
    componentDidMount() {
        $.ajax({
            url: '/Home/GetCoupons',
            success: function (result) {
                CouponsView.setState({
                    results: JSON.parse(result)
                });
                $('#CouponsTables').DataTable({
                    "destroy": true,
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



    handleEditUser(e) {
        e.preventDefault();
        $('#editProject').modal();
    }

    handleDeleteCoupon(Name, e) {
        e.preventDefault();
        $.ajax({
            data: {
                name: Name
            },
            url: '/Home/DeleteCoupon',
            success: function (result) {
                CouponsView.componentDidMount();
            }
        });
    }

    renderTableData() {
        return (
            this.state.results.length > 0 ?
                CouponsView.state.results.map((item, i) => {
                    var module = (item.module.toString().split(',')[0] == "1" ? "Free" : item.module.toString().split(',')[0] == "2" ? "Intraday" : "Positional");
                    return (
                        <tr key={item.id}>
                            <td>{item.name}</td>
                            <td>{item.percent}</td>
                            <td>{item.active}</td>
                            <td>{module}</td>
                            <td>
                                <button className="btn btn-primary btn-xs px-4 mr-2" id="btnEditCoupon" data-obj={JSON.stringify(item)} data-mode={1} onClick={this.handleEditUser.bind(this)}>Edit</button>
                                <button className="btn btn-danger btn-xs px-2" id="btnDelete" onClick={this.handleDeleteCoupon.bind(this, item.name)}>x</button>
                            </td>
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
        console.log("render");
        return (
            <div className="content-wrapper pt-4">
                <section className="content-header">
                    <div className="container-fluid">
                        <div className="row mb-2">
                            <div className="col-sm-6">
                                <h1>Coupon Management</h1>
                            </div>
                            <div className="col-sm-2 ml-auto">
                                <a className="btn btn-outline-primary btn-block" id="btnAddCoupon" data-obj={""} data-mode={0} onClick={this.handleEditUser.bind(this)}>Add Coupon</a>
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
                                        <table id="CouponsTables" className="table table-bordered table-hover">
                                            <thead>
                                                <tr>
                                                    <th>Coupon</th>
                                                    <th>Cost %</th>
                                                    <th>Active</th>
                                                    <th>Module</th>
                                                    <th>Actions</th>
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

var CouponsView = ReactDOM.render(<Coupons />, document.getElementById('Details'));
var AssignCoupon = ReactDOM.render(<EditCoupon />, document.getElementById('AddOrEditCoupon'));