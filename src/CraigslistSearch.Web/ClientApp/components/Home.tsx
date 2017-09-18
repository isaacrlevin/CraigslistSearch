import * as React from 'react';

import { Link, RouteComponentProps } from 'react-router-dom';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux'
import { ApplicationState } from '../store';
import * as ResourcesState from '../store/Resources';
import * as SearchState from '../store/Search';
import Slider, { createSliderWithTooltip } from 'rc-slider';
import * as ReactIntl from 'react-intl';
import { BootstrapTable, TableHeaderColumn } from 'react-bootstrap-table';



var IntlProvider = ReactIntl.IntlProvider;
var FormattedDate = ReactIntl.FormattedDate;

const SliderWithTooltip = createSliderWithTooltip(Slider);
const Range = createSliderWithTooltip(Slider.Range);


type HomeProps =
    ResourcesState.ResourcesState
    & SearchState.SearchState
    & typeof SearchState.actionCreators
    & typeof ResourcesState.actionCreators
    & RouteComponentProps<{}>;

const actionCreators = {
    ...SearchState.actionCreators,
    ...ResourcesState.actionCreators
}


const mapDispatchToProps = (dispatch: any) => {
    return {
        ...bindActionCreators(actionCreators, dispatch)
    }
}

const icon = {
    transition: 'transform 200ms',
    transform: 'rotateY(180deg)',
    // radium provides hover states and vendor prefixes
    ':hover': {
        transform: 'rotateY(180deg) scale(1.5)'
    },
};

class Home extends React.Component<HomeProps, {}> {

    componentWillMount() {
        this.props.getCategories();
        this.props.getLocations();
    }

    filter: SearchState.Filter;

    constructor() {
        super();
        this.options = {
            defaultSortName: 'title',  // default sort column name
            defaultSortOrder: 'asc'  // default sort order
        };
    }

    options: any;

    changeAmount = (value: any) => {
        this.props.changeAmount(value);
    };

    search = (e: any) => {
        this.filter = {
            age: this.props.age,
            location: this.props.location,
            amount: this.props.amount,
            category: this.props.category,
            searchText: this.props.searchText
        };

        e.preventDefault();
        this.props.search(this.filter);
    }
    changeAge = (value: any) => {
        this.props.changeAge(value);
    };

    changeText = (event: any) => {
        this.props.changeSearchText(event.target.value);
    };

    changeLocation = (event: any) => {
        this.props.changeLocation(event.target.value);
    };

    changeCategory = (event: any) => {
        this.props.changeCategory(event.target.value);
    };

    showAge = (v: any) => {
        return `${v}`;
    };

    showAmount = (v: any) => {
        return `$${v}`;
    };

    dateFormatter(cell: any, row: any) {
        var myDate = new Date(cell);
       return myDate.toLocaleString();
    }

    linkFormatter(cell: any, row: any)
    {
        return `<a href='${row.externalUrl}' target='_blank'>${cell}</a>`;
    }

    amountFormatter(value: number[])
    {
        return `$${value[0]} - $${value[1]}`;
    }


    public render() {

        return <div>
           
            <div className="row col-md-12 col-md-offset-2">
                <h2>Search Craigslist</h2>
                <div className="col-xs-4">
                    <label className="block-label">Search</label>
                    <div className="input-group">
                        <input type="textbox" value={this.props.searchText} onChange={this.changeText} />
                    </div> </div>
                <div className="col-xs-4">
                    <label className="block-label">Category</label>
                    <div className="input-group">
                        <select value={this.props.category} onChange={this.changeCategory}>
                            {this.props.categories.map(category =>
                                <option key={category.abbreviation} value={category.abbreviation}>
                                    {category.description}</option>
                            )}</select></div>
                </div>
                <div className="col-xs-4">
                    <label className="block-label">Location</label>
                    <div className="input-group">
                        <select value={this.props.location} onChange={this.changeLocation}>
                            <option key="all" value="all">All</option>
                            {this.props.locations.map(location =>
                                <option key={location.city} value={location.city}>
                                    {location.city}</option>
                            )}</select>
                    </div>
                </div>
            </div>
            <div className="row col-md-12 col-md-offset-2" style={{ marginTop: '20px' }}>
                <div className="col-xs-4">
                    <label className="block-label">Price Range: {this.amountFormatter(this.props.amount)}</label>
                    <div className='slider'>
                        <Range
                            allowCross={false}
                            defaultValue={[0, 10000]}
                            max={10000} min={0}
                            onChange={this.changeAmount}
                            tipFormatter={this.showAmount}
                            tipProps={{ overlayClassName: 'foo' }}
                        />
                    </div>
                </div>
                <div className="col-xs-4"></div>
                <div className="col-xs-4">
                    <label className="block-label">Age of Listing: {this.props.age}</label>
                    <div className='slider'>
                        <SliderWithTooltip
                            defaultValue={2}
                            max={14}
                            tipFormatter={this.showAge}
                            tipProps={{ overlayClassName: 'foo' }}
                            onChange={this.changeAge}
                        />
                    </div>
                </div>
            </div>
            <div className="col-md-12 col-md-offset-2" style={{ marginBottom: '20px', marginTop: '20px' }}>
                <input type="button" value="Get Listings" onClick={this.search} />
                </div>
            {this.renderResultsTable()}
        </div>;
    }

    private renderResultsTable() {

        var table;
        var message;
        if (this.props.isSearching) {
            table = <div><i className="fa fa-circle-o-notch fa-spin fa-3x fa-fw"></i></div>
        }
        else {
            table =                
                <BootstrapTable data={this.props.results} options={this.options} striped hover pagination>
                <TableHeaderColumn dataField='location' dataSort>Location</TableHeaderColumn>
                <TableHeaderColumn dataField='title' dataSort dataFormat={this.linkFormatter} isKey>Title</TableHeaderColumn>
                <TableHeaderColumn dataField='timeStampDate' dataSort dataFormat={this.dateFormatter}>Date Submitted</TableHeaderColumn>
                </BootstrapTable>

            if (this.props.results.length > 0) {
                if (this.props.results.length == 1)
                {
                    message = "1 Listing Found";
                }
                else
                {
                    message = this.props.results.length + " Listings Found";
                }
            }
          
        }
        return <div>
            <div className="row">
                <div className="col-md-12 col-md-offset-7"><strong>{message}</strong></div></div>
            <div className="col-md-12 col-md-offset-2">{table}</div>
        </div>;
    }
}
export default connect(
    (state: ApplicationState) => Object.assign({}, state.search, state.resources),
    mapDispatchToProps
)(Home) as typeof Home;