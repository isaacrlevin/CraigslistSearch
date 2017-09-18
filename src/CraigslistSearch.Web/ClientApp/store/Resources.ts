import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';

export interface ResourcesState {
    isLoading: boolean;
    categories: Category[];
    locations: Location[];
    category: string;
    location: string;
    amount: number[];
    searchText: string;
    age: number;
}

export interface Category {
    description: string;
    abbreviation: string;
}

export interface Location {
    externalUrl: string;
    city: string;
}

interface RequestCategoriesAction {
    type: 'REQUEST_CATEGORY';
}

interface ReceiveCategoriesAction {
    type: 'RECEIVE_CATEGORY';
    categories: Category[];
}

interface RequestLocationsAction {
    type: 'REQUEST_LOCATION';
}

interface ReceiveLocationsAction {
    type: 'RECEIVE_LOCATION';
    locations: Location[];
}

interface ChangeAmountAction {
    type: 'CHANGE_AMOUNT';
    amount: number[]
}

interface ChangeAgeAction {
    type: 'CHANGE_AGE';
    age: number
}

interface ChangeTextAction {
    type: 'CHANGE_TEXT';
    searchText: string
}

interface ChangeLocationAction {
    type: 'CHANGE_LOCATION';
    location: string
}

interface ChangeCategoryAction {
    type: 'CHANGE_CATEGORY';
    category: string
}

type KnownAction = RequestCategoriesAction | ReceiveCategoriesAction | RequestLocationsAction | ReceiveLocationsAction | ChangeAmountAction | ChangeAgeAction | ChangeTextAction | ChangeLocationAction| ChangeCategoryAction;

export const actionCreators = {
    getCategories: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
        let fetchTask = fetch(`api/resource/getcategories`)
            .then(response => {
                return response.json() as Promise<Category[]>
            })
            .then(data => {
                dispatch({ type: 'RECEIVE_CATEGORY', categories: data });
            });

        addTask(fetchTask);
        dispatch({ type: 'REQUEST_CATEGORY' });
    },

    getLocations: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
        let fetchTask = fetch(`api/resource/getlocations`)
            .then(response => {
                return response.json() as Promise<Location[]>
            })
            .then(data => {
                dispatch({ type: 'RECEIVE_LOCATION', locations: data });
            });

        addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
        dispatch({ type: 'REQUEST_LOCATION' });
    },

    changeAmount: (amount: number[]) => <ChangeAmountAction>{ type: 'CHANGE_AMOUNT', amount: amount },
    changeAge: (age: number) => <ChangeAgeAction>{ type: 'CHANGE_AGE', age: age },
    changeSearchText: (searchText: string) => <ChangeTextAction>{ type: 'CHANGE_TEXT', searchText: searchText },    
    changeLocation: (location: string) => <ChangeLocationAction>{ type: 'CHANGE_LOCATION', location: location },
    changeCategory: (category: string) => <ChangeCategoryAction>{ type: 'CHANGE_CATEGORY', category: category }

};

const unloadedState: ResourcesState = { categories: [], locations: [], isLoading: false, amount: [0, 10000], age: 2, searchText: '', category: 'sss', location: '' };

export const reducer: Reducer<ResourcesState> = (state: ResourcesState, incomingAction: Action) => {
    const action = incomingAction as KnownAction;
    switch (action.type) {
        case 'REQUEST_CATEGORY':
            return {
                categories: state.categories,
                locations: state.locations,
                isLoading: true,
                amount: state.amount,
                age: state.age,
                searchText: state.searchText,
                category: state.category,
                location: state.location
            };
        case 'RECEIVE_CATEGORY':
            if (action.categories !== state.categories) {
                return {
                    categories: action.categories,
                    locations: state.locations,
                    isLoading: false,
                    amount: state.amount,
                    age: state.age,
                    searchText: state.searchText,
                    category: state.category,
                    location: state.location
                };
            }
            break;
        case 'REQUEST_LOCATION':
            return {
                locations: state.locations,
                categories: state.categories,
                isLoading: true,
                amount: state.amount,
                age: state.age,
                searchText: state.searchText,
                category: state.category,
                location: state.location
            };
        case 'RECEIVE_LOCATION':
            if (action.locations !== state.locations) {
                return {
                    locations: action.locations,
                    categories: state.categories,
                    isLoading: false,
                    amount: state.amount,
                    age: state.age,
                    searchText: state.searchText,
                    category: state.category,
                    location: state.location
                };
            }
            break;
        case 'CHANGE_AMOUNT':
            return {
                locations: state.locations,
                categories: state.categories,
                amount: action.amount,
                isLoading: false,
                age: state.age,
                searchText: state.searchText,
                category: state.category,
                location: state.location
            };
        case 'CHANGE_AGE':
            return {
                locations: state.locations,
                categories: state.categories,
                amount: state.amount,
                isLoading: false,
                age: action.age,
                searchText: state.searchText,
                category: state.category,
                location: state.location
            };
        case 'CHANGE_TEXT':
            return {
                locations: state.locations,
                categories: state.categories,
                amount: state.amount,
                isLoading: false,
                age: state.age,
                searchText: action.searchText,
                category: state.category,
                location: state.location
            };
        case 'CHANGE_CATEGORY':
            return {
                locations: state.locations,
                categories: state.categories,
                amount: state.amount,
                isLoading: false,
                age: state.age,
                searchText: state.searchText,
                category: action.category,
                location: state.location
            };
        case 'CHANGE_LOCATION':
            return {
                locations: state.locations,
                categories: state.categories,
                amount: state.amount,
                isLoading: false,
                age: state.age,
                searchText: state.searchText,
                category: state.category,
                location: action.location
            };


        default:
            const exhaustiveCheck: never = action;
    }
    return state || unloadedState;
};
