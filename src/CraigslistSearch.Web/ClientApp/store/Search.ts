import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';

export interface SearchState {
    isSearching: boolean;
    results: Result[];
}

export interface Filter {
    age: number;
    amount: number[];
    location: string;
    searchText: string;
    category: string;
}

export interface Result {
    body: string;
    title: string;
    timeStampDate: Date;
    price: number;
    location: string;
    externalUrl: string;
}

interface RequestResultsAction {
    type: 'REQUEST_RESULTS';
}

interface ReceiveResultsAction {
    type: 'RECEIVE_RESULTS';
    results: Result[];
}

type KnownAction = RequestResultsAction | ReceiveResultsAction;

export const actionCreators = {
        search: (filter: Filter): AppThunkAction<KnownAction> => (dispatch, getState) => {
        let fetchTask = fetch('api/search', {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(filter)
        })
            .then(response => {
                return response.json() as Promise<Result[]>
            })
            .then(data => {
                dispatch({ type: 'RECEIVE_RESULTS', results: data });
            });

        addTask(fetchTask);
        dispatch({ type: 'REQUEST_RESULTS' });
    }
};

const unloadedState: SearchState = { results: [], isSearching: false };

export const reducer: Reducer<SearchState> = (state: SearchState, incomingAction: Action) => {
    const action = incomingAction as KnownAction;
    switch (action.type) {
        case 'REQUEST_RESULTS':
            return {
                results: state.results,
                isSearching: true
            };
        case 'RECEIVE_RESULTS':
            if (action.results !== state.results) {
                return {
                    results: action.results,
                    isSearching: false
                };
            }
            break;
        default:
            const exhaustiveCheck: never = action;
    }
    return state || unloadedState;
};
