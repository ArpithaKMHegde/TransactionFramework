import { Action, Reducer } from 'redux';
import { AppThunkAction } from '.';
import { Member } from './Members';
import { Product } from './Products';
import { User } from './Users';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface TransactionsState {
    isLoading: boolean;
    startDateIndex?: number;
    transactions: Transaction[];
    uivalues: Transaction;
}

export interface Transaction {
    TransactionId: number;
    Invoice: Sales;
    TotalAmount: number;
    PaymentType: string;
    TransactionStatus: number;
    TransactionDate: Date;
    lMemberId: number;
    lFirstName: string;
    lProductId: number;
    lProductNumber: string;
    lActiveProduct: Product;
    SoldQuantity: number;
    TransactionMessage: string;
}

export interface Sales
{
    SalesId: number;
    BuyingMember: Member;
    SalesDateTime: Date;
    ReceiptNumber: string;
    DiscountAmount: number;
    Items: SalesDetail[];
    TotalBillAmount: number;
}

export interface SalesDetail
{
    SalesDetailsId: number;
    SoldProduct: Product;
    Quantity: number;
    UnitPrice: number;
    TotalPrice: number;
    IsDeleted: number;
}

export interface GetSingleItems
{
    Id : number;
    typeName : string;
    vProduct : Product;
    vTransaction : Transaction;
    vMember : Member;
    vUser : User;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

export interface RequestTransactionsAction {
    type: 'REQUEST_TRANSACTIONS';
    startDateIndex: number;
}

export interface ReceiveTransactionsAction {
    type: 'RECEIVE_TRANSACTIONS';
    startDateIndex: number;
    transactions: Transaction[];
}

export interface SaveStartAction {
    type: 'SAVE_START';
}
export interface SaveCompleteAction {
    type: 'SAVE_COMPLETE';
    data: Transaction;
}

export interface GetCustomerRequestAction {
    type: 'GET_MEMBER_REQUEST';
}

export interface GetCustomerResponseAction {
    type: 'GET_MEMBER_RESPONSE';
    data: Member;
}

export interface AddProductToListAction
{
    type: 'ADD_PRODUCT_TO_LIST';
}
export interface DeleteProductFromListAction
{
    type: 'DELETE_PRODUCT_FROM_LIST';
    salesdetailid: number;
    productid:number;
}

export interface GetProductRequestAction
{
    type: 'GET_PRODUCT_REQUEST';
}
export interface GetProductResponseAction
{
    type: 'GET_PRODUCT_RESPONSE';
    data: Product;
}

export interface ChangeTransactionValuesAction { 
    type: 'CHANGES_TEXT'; 
    controlName: string; 
    controlValue: string;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = RequestTransactionsAction | ReceiveTransactionsAction | ChangeTransactionValuesAction | SaveStartAction 
                    | SaveCompleteAction | GetCustomerRequestAction | GetCustomerResponseAction | AddProductToListAction
                    | GetProductRequestAction | GetProductResponseAction | DeleteProductFromListAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
    requestTransactions: (startDateIndex: number): AppThunkAction<KnownAction> => (dispatch, getState) => {
        // Only load data if it's something we don't already have (and are not already loading)
        const appState = getState();
        if (appState && appState.transactions ) {
            fetch(`Transaction`)
                .then(response => response.json() as Promise<Transaction[]>)
                .then(data => {
                    dispatch({ type: 'RECEIVE_TRANSACTIONS', startDateIndex: startDateIndex, transactions: data });
                });

            dispatch({ type: 'REQUEST_TRANSACTIONS', startDateIndex: startDateIndex });
        }
    },
    transactionValueChange: (changedControl: string, changedValue: string) => ({ type: 'CHANGES_TEXT', controlName: changedControl, controlValue: changedValue } as ChangeTransactionValuesAction),
    saveTransaction:  (): AppThunkAction<KnownAction> => (dispatch, getState) => {
        // Only load data if it's something we don't already have (and are not already loading)
        const appState = getState();
        if (appState && appState.transactions) {
            fetch(`Transaction`,
            {  
                method: 'post',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify(appState.transactions.uivalues),
            })
            .then(response => response.json() as Promise<Transaction>)
            .then(data => {
                dispatch({ type: 'SAVE_COMPLETE', data:data});
            });
            dispatch({ type: 'SAVE_START'});
        }
    },
    getMemberName: () : AppThunkAction<KnownAction> => (dispatch, getState) =>{
        const appState = getState();
        if (appState && appState.transactions) {
            fetch(`SingleItemById`,
            {  
                method: 'post',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({Id:appState.transactions.uivalues.lMemberId, typeName: 'member'}),
            })
            .then(response => response.json() as Promise<GetSingleItems>)
            .then(data => {
                dispatch({ type: 'GET_MEMBER_RESPONSE', data: data.vMember});
            });

            dispatch({ type: 'GET_MEMBER_REQUEST'});
        }
    },
    GetProduct: () : AppThunkAction<KnownAction> => (dispatch, getState) =>{
        const appState = getState();
        if (appState && appState.transactions) {
            fetch(`SingleItemById`,
            {  
                method: 'post',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({Id:appState.transactions.uivalues.lProductId, typeName: 'product'}),
            })
            .then(response => response.json() as Promise<GetSingleItems>)
            .then(data => {
                dispatch({ type: 'GET_PRODUCT_RESPONSE', data:data.vProduct});
            });

            dispatch({ type: 'GET_PRODUCT_REQUEST'});
        }
    },
    AddProductToList: () => ({ type: 'ADD_PRODUCT_TO_LIST' } as AddProductToListAction),
    DeleteProductFromList: (salesdetailid: number,productid:number) => ({ type: 'DELETE_PRODUCT_FROM_LIST', salesdetailid: salesdetailid, productid:productid } as DeleteProductFromListAction),
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.
const noMemberState = {
    ContactNumber: "", EmailId: "", FirstName: "", MemberId: 0, LastName: "", Address: "", StartDate: "", EndDate: ""
}

const noProductState = {
    ProductId: 0, ProductNumber: '', ProductName: '', ProductDescription: '',
    Brand: '', MemberId: 0, Quantity: 0, PricePerUnit:5.0
}

const noTransactionstate = {
    TransactionId:0, 
    PaymentType:'Cash', 
    TransactionDate:new Date(), 
    TransactionStatus:0,
    Invoice: {
        BuyingMember :noMemberState,
        DiscountAmount : 0.0,
        Items :[],
        ReceiptNumber: (Math.floor(Math.random() * 90000) + 10000).toString(),
        SalesDateTime: new Date(),
        SalesId: 0,
        TotalBillAmount: 0.0
    },
    TotalAmount : 0,
    lMemberId : 0,
    lFirstName:'',
    lProductId : 0,
    lProductNumber:'',
    lActiveProduct: noProductState,
    SoldQuantity:0,
    TransactionMessage:''
} ;
const unloadedState: TransactionsState = { 
    transactions: [], 
    isLoading: false, 
    uivalues: noTransactionstate
}

export const reducer: Reducer<TransactionsState> = (state: TransactionsState | undefined, incomingAction: Action): TransactionsState => {
    if (state === undefined) {
        return unloadedState;
    }

    const action = incomingAction as KnownAction;
    switch (action.type) {
        case 'REQUEST_TRANSACTIONS':
            return {
                startDateIndex: action.startDateIndex,
                transactions: state.transactions,
                isLoading: true,
                uivalues: state.uivalues
            };
        case 'RECEIVE_TRANSACTIONS':
            // Only accept the incoming data if it matches the most recent request. This ensures we correctly
            // handle out-of-order responses.
            if (action.startDateIndex === state.startDateIndex) {
                return {
                    startDateIndex: action.startDateIndex,
                    transactions: action.transactions,
                    isLoading: false,
                    uivalues: state.uivalues
                };
            };
            break;
        case 'CHANGES_TEXT':
            let lUivalues = {...state.uivalues};
            switch(action.controlName)
            {
                case 'PaymentType':
                    lUivalues.PaymentType = action.controlValue;
                    break;
                case 'TransactionDate':
                    lUivalues.TransactionDate = new Date(action.controlValue);
                    break;
                case 'TransactionStatus':
                    lUivalues.TransactionStatus = Number(action.controlValue);
                    break;
                case 'lMemberId':
                    lUivalues.lMemberId  = Number(action.controlValue);
                    break;
                case 'lFirstName':
                    lUivalues.lFirstName  = action.controlValue;
                    lUivalues.Invoice.BuyingMember.FirstName  = action.controlValue;
                    lUivalues.Invoice.BuyingMember.LastName  = '';
                    lUivalues.Invoice.BuyingMember.ContactNumber  = '';
                    lUivalues.Invoice.BuyingMember.Address  = '';
                    lUivalues.Invoice.BuyingMember.EmailId  = '';
                    lUivalues.Invoice.BuyingMember.StartDate  = '01/01/1970';
                    lUivalues.Invoice.BuyingMember.EndDate  = '01/01/1970';
                    lUivalues.Invoice.BuyingMember.ContactNumber  = '';
                    break;
                case 'lProductId':
                    lUivalues.lProductId  = Number(action.controlValue);
                    break;
                case 'lProductNumber':
                    lUivalues.lProductNumber  = action.controlValue;
                    lUivalues.lActiveProduct.ProductNumber = action.controlValue 
                    break;
                case 'Quantity':
                        lUivalues.SoldQuantity = Number(action.controlValue);
                    break;
                case 'ReceiptNumber':
                        lUivalues.Invoice.ReceiptNumber = action.controlValue;
                        break;
                default:
                    break;
            }
            return  {
                startDateIndex: state.startDateIndex,
                transactions: state.transactions,
                isLoading: false,
                uivalues: lUivalues
            }
        case 'SAVE_START':
            return {...state};
        case 'SAVE_COMPLETE':
            let msg = action.data.TransactionMessage;
            let lsUIValues = {...noTransactionstate, TransactionMessage:msg}
            return {...state, uivalues:lsUIValues};
        case 'GET_MEMBER_REQUEST':
            let lfUivalues = {...state.uivalues, TransactionMessage:''};
            lfUivalues.Invoice.BuyingMember = noMemberState;
            lfUivalues.lFirstName = '';
            return {...state, uivalues: lfUivalues };
        case 'GET_MEMBER_RESPONSE':
            let lmUivalues = {...state.uivalues, TransactionMessage:''};
            lmUivalues.Invoice.BuyingMember = action.data;
            lmUivalues.lFirstName = action.data.FirstName;
            return {...state, uivalues: lmUivalues };
        case 'GET_PRODUCT_REQUEST':
            let lpdUivalues = {...state.uivalues, TransactionMessage:''};
            lpdUivalues.lProductNumber = '';
            return {...state, uivalues: lpdUivalues };
        case 'GET_PRODUCT_RESPONSE':
            let lpUivalues = {...state.uivalues, TransactionMessage:''};
            lpUivalues.lActiveProduct = action.data;
            lpUivalues.lProductNumber = action.data.ProductNumber;
            lpUivalues.TotalAmount = lpUivalues.Invoice.Items.reduce((sum, current) => sum + (current.SoldProduct.PricePerUnit * current.Quantity), 0);
            return {...state, uivalues: lpUivalues};
        case 'ADD_PRODUCT_TO_LIST':
            let lpaUivalues = {...state.uivalues, TransactionMessage:''};
            let salesItem : SalesDetail = {SoldProduct: state.uivalues.lActiveProduct,  
                Quantity: state.uivalues.SoldQuantity,
                SalesDetailsId : 0, 
                TotalPrice: state.uivalues.lActiveProduct.PricePerUnit,
                UnitPrice : state.uivalues.lActiveProduct.PricePerUnit,
                IsDeleted: 0
            }
            lpaUivalues.Invoice.Items.push(salesItem);
            lpaUivalues.Invoice.DiscountAmount = lpaUivalues.Invoice.Items.reduce((totDiscount, current) => totDiscount + ((current.SoldProduct.PricePerUnit * current.Quantity) * 0.05), 0);
            lpaUivalues.TotalAmount = (lpaUivalues.Invoice.Items.reduce((sum, current) => sum + (current.SoldProduct.PricePerUnit * current.Quantity), 0)) - lpaUivalues.Invoice.DiscountAmount;
            return {...state, uivalues: lpaUivalues};
        case 'DELETE_PRODUCT_FROM_LIST':
            let lpdtUivalues = {...state.uivalues, TransactionMessage:''};
            lpdtUivalues.Invoice.Items = [...lpdtUivalues.Invoice.Items.filter(it => !(it.SalesDetailsId === action.salesdetailid && it.SoldProduct.ProductId === action.productid) ) ]
            lpdtUivalues.Invoice.DiscountAmount = lpdtUivalues.Invoice.Items.reduce((totDiscount, current) => totDiscount + ((current.SoldProduct.PricePerUnit * current.Quantity) * 0.05), 0);
            lpdtUivalues.TotalAmount = (lpdtUivalues.Invoice.Items.reduce((sum, current) => sum + (current.SoldProduct.PricePerUnit * current.Quantity), 0)) - lpdtUivalues.Invoice.DiscountAmount;
            return {...state, uivalues: lpdtUivalues};
        default:
            return {...state};
    }

    return {...state};
};
