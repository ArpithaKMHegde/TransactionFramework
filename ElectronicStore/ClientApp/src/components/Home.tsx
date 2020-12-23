import * as React from 'react';
import { connect } from 'react-redux';

const Home = () => (
  <div>
    <h1>Hello, this is a Demo Application For Transaction Integrity Challenges Of Microservices and the Framework Approach  !</h1>
    <p>Welcome to your store of computers selling platform. Note this applications is built with:</p>
    <ul>
      <li><a href='https://get.asp.net/'>ASP.NET Core</a> and <a href='https://msdn.microsoft.com/en-us/library/67ef8sbd.aspx'>C#</a> for cross-platform server-side code</li>
      <li><a href='https://facebook.github.io/react/'>React</a> and <a href='https://redux.js.org/'>Redux</a> for client-side code</li>
      <li><a href='http://getbootstrap.com/'>Bootstrap</a> for layout and styling</li>
    </ul>
    <p>To help you get started, we've also set up:</p>
    <ul>
      <li><strong>Product management</strong>. Click on <em>Products</em> and browsers <em>Back</em> button to return here.</li>
      <li><strong>Members</strong>. These two are links where administrator can add customers of the application.</li>
      <li><strong>Transactions</strong>. All selling and records of transactions are managed here.</li>
    </ul>
  </div>
);

export default connect()(Home);
