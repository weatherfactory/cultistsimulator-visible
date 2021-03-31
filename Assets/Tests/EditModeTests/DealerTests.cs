using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Logic;
using NUnit.Framework;
using SecretHistories.Abstract;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Logic;
using SecretHistories.Services;
using SecretHistories.Spheres;
using SecretHistories.Spheres.SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace Assets.Tests.EditModeTests
{


    [TestFixture]
    public class DealerTests
    {
        Element element;
        ElementStack elementStack;
        Token token;
        DummyDealersTable dt;
        Dealer dealer;
        DeckSpec deckSpec;
        const string drawmessage = "One to three is sufficient.";



        [SetUp]
        public void Setup()
        {
            element = new Element();
            element.SetId("x");
            elementStack=new ElementStack("x1",element,1,Timeshadow.CreateTimelessShadow(), Context.Unknown());
            token = new GameObject().AddComponent<Token>();
            token.SetPayload(elementStack);
            dt = new DummyDealersTable();
            dt.GetDrawPile("any string here").AcceptToken(token,Context.Unknown());
            dealer = new Dealer(dt);
            
            
            deckSpec = new DeckSpec();
            deckSpec.ResetOnExhaustion = true;
            deckSpec.DrawMessages.Add(elementStack.EntityId,drawmessage);
        }

        [Test]
        public void CanDealCard()
        {
            Assert.IsInstanceOf(typeof(Token),   dealer.Deal(deckSpec));
        }

        [Test]
        public void DrawnCardIsIlluminatedWithDrawMessage()
        {
            var dealtCard = dealer.Deal(deckSpec);
            var messageIllumination = dealtCard.Payload.GetIllumination(NoonConstants.MESSAGE_ILLUMINATION_KEY);
            Assert.AreEqual(drawmessage,messageIllumination);
        }


        class DummyDealersTable : IHasCardPiles
        {
            private DummySphere _drawPile;
          private DummySphere _forbiddenPile;

          public DummyDealersTable()
          {
              _drawPile= new DummySphere();

                _forbiddenPile = new DummySphere();


            }
            public IEnumerable<IHasElementTokens> GetDrawPiles()
            {
                return new List<IHasElementTokens> {_drawPile};
            }

            public IHasElementTokens GetDrawPile(string forDeckSpecId)
            {
                return _drawPile;
            }

            public IHasElementTokens GetForbiddenPile(string forDeckSpecId)
            {
                return _forbiddenPile;
            }
        }

        class DummySphere : IHasElementTokens
        {
            private List<Token> _elementTokens=new List<Token>();


            public string GetDeckSpecId()
            {
                throw new NotImplementedException();
            }

            public List<Token> GetElementTokens()
            {
                return _elementTokens;
            }

            public int GetTotalStacksCount()
            {
                return _elementTokens.Count;
            }

            public void RetireTokensWhere(Func<Token, bool> filter)
            {
                throw new NotImplementedException();
            }

            public void AcceptToken(Token t, Context c)
            {
               _elementTokens.Add(t);
            }

            public Token ProvisionElementToken(string card, int i)
            {
                throw new NotImplementedException();
            }
        }

    }
}
